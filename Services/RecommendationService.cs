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
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MealRecommendationSettings _settings;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public RecommendationService(IUnitOfWork unitOfWork, IOptions<MealRecommendationSettings> options, IMapper mapper, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _settings = options.Value;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        // Generate meal recommendations for a user
        public async Task<IEnumerable<MealResponse>> GenerateRecommendationsAsync()
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var userPreferences = await _unitOfWork.Repository<UserPreference>()
                    .FirstOrDefaultAsync(up => up.SmartDietUserId == userId)
                    ?? throw new Exception("User preferences not found");

                var userAllergies = await _unitOfWork.Repository<UserAllergy>()
                    .FindAsync(ua => ua.SmartDietUserId == userId);

                var recentMeals = await _unitOfWork.Repository<MealRecommendationHistory>()
                    .FindAsync(r => r.SmartDietUserId == userId &&
                        r.RecommendationDate > DateTime.UtcNow.AddDays(-_settings.DaysToExcludeRecentlyRecommended));

                var recentMealIds = recentMeals.Select(r => r.MealId).ToHashSet();

                var allMeals = await _unitOfWork.Repository<Meal>().GetAllAsync(
                    //includes:
                    //[
                    //    m => m.MealDishes,
                    //    m => m.MealDishes.Select(md => md.Dish),
                    //    m => m.MealDishes.Select(md => md.Dish.DishIngredients),
                    //    m => m.MealDishes.Select(md => md.Dish.DishIngredients.Select(di => di.Food))
                    //]
                    include: query =>
    query.Include(m => m.MealDishes)
         .ThenInclude(md => md.Dish)
         .ThenInclude(d => d.DishIngredients)
         .ThenInclude(di => di.Food)

                    );

                var filteredMeals = allMeals.Where(m =>
                    //!recentMealIds.Contains(m.Id) &&
                    m.MealDishes.Any(md =>

                        md.Dish.DietType == userPreferences.PrimaryDietType
                        &&
                        (userPreferences.PrimaryRegionType.HasFlag(md.Dish.RegionType) ||
                         md.Dish.RegionType.HasFlag(userPreferences.PrimaryRegionType))
                         &&
                        md.Dish.CookingTimeMinutes <= userPreferences.MaxCookingTime
                        &&
                        md.Dish.Difficulty <= userPreferences.MaxRecipeDifficulty
                        &&
                        !md.Dish.DishIngredients.Any(di => userAllergies.Any(ua => ua.FoodId == di.FoodId))

                    )).ToList();

                var scoredMeals = filteredMeals.Select(m => new
                {
                    Meal = m,
                    Score = CalculateMealScore(m, userId)
                }).OrderByDescending(m => m.Score).ToList();

                var recommendedMeals = scoredMeals.Take(userPreferences.DailyMealCount).Select(m => m.Meal).ToList();

                // Save recommendations to history
                foreach (var meal in recommendedMeals)
                {
                    await _unitOfWork.Repository<MealRecommendationHistory>().AddAsync(new MealRecommendationHistory
                    {
                        Id = Guid.NewGuid().ToString(),
                        SmartDietUserId = userId,
                        MealId = meal.Id,
                        CreatedBy = userId,
                        RecommendationDate = DateTime.UtcNow
                    });
                }

                await _unitOfWork.SaveChangeAsync();

                return _mapper.Map<IEnumerable<MealResponse>>(recommendedMeals);
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Log the exception and rethrow or handle it as needed
                throw new Exception("Failed to generate recommendations", ex);
            }
        }

        // Regenerate recommendations for a user
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

        // Get current recommended meals for a user
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

        // Get recommendation history for a user
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

        // Calculate the score for a meal
        private double CalculateMealScore(Meal meal, string userId)
        {
            var mealRatingPoints = meal.AverageRating * _settings.Points.MealRatingPerStar;

            var userInteraction = _unitOfWork.Repository<UserMealInteraction>()
                .FirstOrDefaultAsync(umi => umi.SmartDietUserId == userId && umi.MealId == meal.Id).Result;

            var interactionPoints = userInteraction?.InteractionType switch
            {
                InteractionType.Liked => _settings.Points.LikedMeal,
                InteractionType.Disliked => _settings.Points.DislikedMeal,
                _ => 0
            };

            var favoriteDishPoints = meal.MealDishes.Sum(md =>
                _unitOfWork.Repository<FavoriteDish>()
                    .AnyAsync(fd => fd.SmartDietUserId == userId && fd.DishId == md.DishId).Result
                    ? _settings.Points.FavoriteDishPerDish : 0);

            var newMealPoints = userInteraction == null ? _settings.Points.NewMeal : 0;

            return mealRatingPoints + interactionPoints + favoriteDishPoints + newMealPoints;
        }
    }
}