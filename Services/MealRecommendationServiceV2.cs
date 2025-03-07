using AutoMapper;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using BusinessObjects.Enum;
using BusinessObjects.Exceptions;
using DTOs.MealDTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{

    public class MealRecommendationServiceV2 : IMealRecommendationServiceV2
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MealRecommendationSettings _settings;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly Random _random;

        public MealRecommendationServiceV2(
            IUnitOfWork unitOfWork,
            IOptions<MealRecommendationSettings> options,
            IMapper mapper,
            ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _settings = options.Value;
            _mapper = mapper;
            _tokenService = tokenService;
            _random = new Random();
        }

        public async Task<IEnumerable<MealResponse>> GenerateRecommendationsAsync()
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                // Parallel fetching of user data
                var (userPreferences, userAllergies, recentMealIds) = await GetUserDataAsync(userId);
                var (userInteractions, favoriteDishIds) = await GetUserInteractionDataAsync(userId);

                // Get meals with optimized query
                var meals = await GetEligibleMealsAsync(userId, recentMealIds, userPreferences, userAllergies);

                // Calculate scores with diversity factors
                var scoredMeals = meals.Select(m => new
                {
                    Meal = m,
                    Score = CalculateEnhancedScore(m, userInteractions, favoriteDishIds)
                }).OrderByDescending(m => m.Score).ToList();

                // Apply diversity-aware selection
                var recommendedMeals = SelectDiverseMeals(scoredMeals, userPreferences.DailyMealCount);

                // Save recommendations
                await SaveRecommendationsAsync(userId, recommendedMeals);

                return _mapper.Map<IEnumerable<MealResponse>>(recommendedMeals);
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to generate recommendations", ex);
            }
        }

        private async Task<(UserPreference, IEnumerable<UserAllergy>, HashSet<string>)> GetUserDataAsync(string userId)
        {
            var userPreferences = await _unitOfWork.Repository<UserPreference>()
                .FirstOrDefaultAsync(up => up.SmartDietUserId == userId)
                ?? throw new Exception("User preferences not found");

            var userAllergies = await _unitOfWork.Repository<UserAllergy>()
                .FindAsync(ua => ua.SmartDietUserId == userId);

            var recentMeals = await _unitOfWork.Repository<MealRecommendationHistory>()
                .FindAsync(r => r.SmartDietUserId == userId &&
                    r.RecommendationDate > DateTime.UtcNow.AddDays(-_settings.DaysToExcludeRecentlyRecommended));

            return (userPreferences, userAllergies, recentMeals.Select(r => r.MealId).ToHashSet());
        }

        private async Task<(IEnumerable<UserMealInteraction>, List<string>)> GetUserInteractionDataAsync(string userId)
        {
            var userInteractions = await _unitOfWork.Repository<UserMealInteraction>()
                .FindAsync(umi => umi.SmartDietUserId == userId);

            var favoriteDishes = await _unitOfWork.Repository<FavoriteDish>()
                .FindAsync(fd => fd.SmartDietUserId == userId);
            var favoriteDishIds = favoriteDishes.Select(fd => fd.DishId).ToList();

            return (userInteractions, favoriteDishIds);
        }

        private async Task<IEnumerable<Meal>> GetEligibleMealsAsync(
            string userId,
            HashSet<string> recentMealIds,
            UserPreference preferences,
            IEnumerable<UserAllergy> allergies)
        {
            var meal = await _unitOfWork.Repository<Meal>().GetAllAsync(
                include: query => query
                    .Include(m => m.MealDishes)
                        .ThenInclude(md => md.Dish)
                        .ThenInclude(d => d.DishIngredients));
            return meal.Where(m =>
            //!recentMealIds.Contains(m.Id) &&
                    m.MealDishes.Any(md =>
                        md.Dish.DietType == preferences.PrimaryDietType &&
                        md.Dish.RegionType == preferences.PrimaryRegionType &&
                        md.Dish.CookingTimeMinutes <= preferences.MaxCookingTime &&
                        //md.Dish.Difficulty <= preferences.MaxRecipeDifficulty &&
                        !md.Dish.DishIngredients.Any(di => allergies.Any(ua => ua.FoodId == di.FoodId))
                        )).ToList();
        }

        private double CalculateEnhancedScore(
            Meal meal,
            IEnumerable<UserMealInteraction> interactions,
            List<string> favoriteDishIds)
        {
            var interaction = interactions.FirstOrDefault(i => i.MealId == meal.Id);

            // Base scoring
            var score = meal.AverageRating * _settings.Points.MealRatingPerStar
                + (interaction?.InteractionType switch
                {
                    InteractionType.Liked => _settings.Points.LikedMeal,
                    InteractionType.Disliked => _settings.Points.DislikedMeal,
                    _ => 0
                })
                + meal.MealDishes.Sum(md =>
                    favoriteDishIds.Contains(md.DishId)
                        ? _settings.Points.FavoriteDishPerDish
                        : 0)
                + (interaction == null
                    ? _settings.Points.NewMeal
                    : 0);

            // Diversity factors
            score += _random.NextDouble() * 0.5; // Random boost
            score -= CalculateSimilarityPenalty(meal); // Diversity penalty

            return score;
        }

        private List<Meal> SelectDiverseMeals(IEnumerable<dynamic> scoredMeals, int count)
        {
            // Group by diet type first
            
            var grouped = scoredMeals
                .GroupBy(
                m => m.Meal.MealDishes.FirstOrDefault()?.Dish.DietType
               
                )
                .SelectMany(g => g.Take(_settings.MaxMealsPerCategory))
                .ToList();

            // Apply MMR-like selection
            var selected = new List<Meal>();
            var remaining = new Queue<dynamic>(grouped.OrderByDescending(m => m.Score));

            while (selected.Count < count && remaining.Count > 0)
            {
                var next = remaining.Dequeue();
                if (!IsTooSimilar(next.Meal, selected))
                {
                    selected.Add(next.Meal);
                }
            }

            return selected.Take(count).ToList();
        }

        private double CalculateSimilarityPenalty(Meal meal)
        {
            // Implement your similarity calculation logic
            // Example: 0.1 penalty per matching ingredient with already selected meals
            return 0; // Placeholder
        }

        private bool IsTooSimilar(Meal candidate, List<Meal> selected)
        {
            // Implement similarity check logic
            // Example: Check ingredient overlap
            return false; // Placeholder
        }

        private async Task SaveRecommendationsAsync(string userId, IEnumerable<Meal> meals)
        {
            var history = meals.Select(m => new MealRecommendationHistory
            {
                Id = Guid.NewGuid().ToString(),
                SmartDietUserId = userId,
                MealId = m.Id,
                CreatedBy = userId,
                RecommendationDate = DateTime.UtcNow
            }).ToList();

            await _unitOfWork.Repository<MealRecommendationHistory>().AddRangeAsync(history);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task<IEnumerable<MealResponse>> RegenerateRecommendationsAsync()
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                // Clear recent recommendations
                var recentRecommendations = await _unitOfWork.Repository<MealRecommendationHistory>()
                    .FindAsync(r => r.SmartDietUserId == userId &&
                        r.RecommendationDate > DateTime.UtcNow.AddDays(-_settings.DaysToExcludeRecentlyRecommended));

                _unitOfWork.Repository<MealRecommendationHistory>().DeleteRangeAsync(recentRecommendations);
                await _unitOfWork.SaveChangeAsync();

                // Generate new recommendations
                return await GenerateRecommendationsAsync();
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Log the exception and rethrow or handle it as needed
                throw new Exception("Failed to regenerate recommendations", ex);
            }
        }

        public async Task<IEnumerable<MealResponse>> GetRecommendedMealsAsync()
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var recentRecommendations = await _unitOfWork.Repository<MealRecommendationHistory>()
                    .FindAsync(r => r.SmartDietUserId == userId &&
                        r.RecommendationDate > DateTime.UtcNow.AddDays(-_settings.DaysToExcludeRecentlyRecommended),
                        include: query => query.Include(x => x.Meal)
                        .ThenInclude(x => x.MealDishes)
                        );

                return _mapper.Map<IEnumerable<MealResponse>>(recentRecommendations.Select(r => r.Meal));
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Log the exception and rethrow or handle it as needed
                throw new Exception("Failed to get recommended meals", ex);
            }
        }

        public async Task<IEnumerable<MealResponse>> GetRecommendationHistoryAsync()
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var recommendationHistory = await _unitOfWork.Repository<MealRecommendationHistory>()
                    .FindAsync(r => r.SmartDietUserId == userId,
                        include: query => query.Include(x => x.Meal)
                        .ThenInclude(x => x.MealDishes));

                return _mapper.Map<IEnumerable<MealResponse>>(recommendationHistory.Select(r => r.Meal));
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Log the exception and rethrow or handle it as needed
                throw new Exception("Failed to get recommendation history", ex);
            }
        }
    }
}
