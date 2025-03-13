using AutoMapper;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTOs.DishDTOs;
using Repositories.Interfaces;
using BusinessObjects.Exceptions;
using BusinessObjects.Enum;
using BusinessObjects.FixedData;

namespace SmartDietAPI.Services
{
    public class DishRecommendationService : IDishRecommendationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly DishRecommendationSettings _settings;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public DishRecommendationService(IUnitOfWork unitOfWork,
            IOptions<DishRecommendationSettings> options,
            IMapper mapper,
            ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _settings = options.Value;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task<IEnumerable<DishResponse>> GenerateRecommendationsAsync()
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                // Get user preferences
                var userPreferences = await _unitOfWork.Repository<UserPreference>()
                    .FirstOrDefaultAsync(up => up.SmartDietUserId == userId)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, 
                        ErrorCode.NOT_FOUND, 
                        "User preferences not found");

                // Get user allergies
                var userAllergies = await _unitOfWork.Repository<UserAllergy>()
                    .FindAsync(ua => ua.SmartDietUserId == userId);

                // Get recently recommended dishes
                var recentDishes = await _unitOfWork.Repository<DishRecommendHistory>()
                    .FindAsync(r => r.SmartDietUserId == userId &&
                        r.RecommendationDate > DateTime.UtcNow.AddDays(-_settings.DaysToExcludeRecentlyRecommended));

                var recentDishIds = recentDishes.Select(r => r.DishId).ToHashSet();

                // Get all dishes with includes
                var allDishes = await _unitOfWork.Repository<Dish>().GetAllAsync(
                    include: query => query
                        .Include(d => d.DishIngredients)
                        .ThenInclude(di => di.Food)
                );

                // Filter dishes based on user preferences
                var filteredDishes = allDishes.Where(d =>
                    !recentDishIds.Contains(d.Id) &&
                    d.DietType == userPreferences.PrimaryDietType &&
                    (userPreferences.PrimaryRegionType == RegionType.None || 
                     d.RegionType == RegionType.None ||
                     userPreferences.PrimaryRegionType.HasFlag(d.RegionType) ||
                     d.RegionType.HasFlag(userPreferences.PrimaryRegionType)) &&
                    d.CookingTimeMinutes <= userPreferences.MaxCookingTime &&
                    d.Difficulty <= userPreferences.MaxRecipeDifficulty &&
                    !d.DishIngredients.Any(di => userAllergies.Any(ua => ua.FoodId == di.FoodId))
                ).ToList();

                if (!filteredDishes.Any())
                {
                    throw new ErrorException(StatusCodes.Status404NotFound,
                        ErrorCode.NOT_FOUND,
                        "No dishes match current preferences");
                }

                // Score and sort dishes
                var scoredDishes = filteredDishes.Select(d => new
                {
                    Dish = d,
                    Score = CalculateDishScore(d, userId)
                }).OrderByDescending(d => d.Score).ToList();

                // Take top N dishes
                var recommendedDishes = scoredDishes
                    .Take(_settings.MaxDishesToRecommend)
                    .Select(d => d.Dish)
                    .ToList();

                // Save recommendations to history
                foreach (var dish in recommendedDishes)
                {
                    await _unitOfWork.Repository<DishRecommendHistory>().AddAsync(new DishRecommendHistory
                    {
                        Id = Guid.NewGuid().ToString(),
                        SmartDietUserId = userId,
                        DishId = dish.Id,
                        CreatedBy = userId,
                        RecommendationDate = DateTime.UtcNow
                    });
                }

                await _unitOfWork.SaveChangeAsync();

                return _mapper.Map<IEnumerable<DishResponse>>(recommendedDishes);
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Log full error details
                Console.WriteLine($"ERROR in GenerateRecommendationsAsync: {ex}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                throw new ErrorException(StatusCodes.Status500InternalServerError,
                    ErrorCode.INTERNAL_SERVER_ERROR,
                    $"Recommendation failed: {ex.Message}");
            }
        }

        public async Task<IEnumerable<DishResponse>> GetRecommendedDishesAsync()
        {
            try
            {
                Console.WriteLine("1 GetRecommendedDishesAsync"); // Log 1
                var userId = _tokenService.GetUserIdFromToken();
                Console.WriteLine($"UserId: {userId}"); // Log 2
                var recentRecommendations = await _unitOfWork.Repository<DishRecommendHistory>()
                    .FindAsync(r => r.SmartDietUserId == userId,
                        include: query => query.Include(x => x.Dish)
                        .ThenInclude(x => x.DishIngredients)
                        );
                Console.WriteLine($"Count recentRecommendations: {recentRecommendations.Count()}"); // Log 3

                Console.WriteLine("Bắt đầu mapping DishResponse"); // Log 4
                var dishResponses = _mapper.Map<IEnumerable<DishResponse>>(recentRecommendations.Select(r => r.Dish));
                Console.WriteLine("Kết thúc mapping DishResponse"); // Log 5
                return dishResponses;
            }
            catch(ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Ghi log exception gốc để debug
                Console.WriteLine($"Lỗi trong GetRecommendedDishesAsync: {ex}");
                throw new ErrorException(StatusCodes.Status500InternalServerError,
                    ErrorCode.INTERNAL_SERVER_ERROR,
                    "Lỗi khi lấy danh sách món ăn được đề xuất. Xem log để biết thêm chi tiết.");
            }
        }

        public async Task<IEnumerable<DishResponse>> RegenerateRecommendationsAsync()
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                // Clear recent recommendations
                var recentRecommendations = await _unitOfWork.Repository<DishRecommendHistory>()
                    .FindAsync(r => r.SmartDietUserId == userId &&
                        r.RecommendationDate > DateTime.UtcNow.AddDays(-_settings.DaysToExcludeRecentlyRecommended));

                _unitOfWork.Repository<DishRecommendHistory>().DeleteRangeAsync(recentRecommendations);
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
                throw new ErrorException(StatusCodes.Status500InternalServerError,
                    ErrorCode.INTERNAL_SERVER_ERROR,
                    "Failed to regenerate dish recommendations");
            }
        }

        public async Task<IEnumerable<DishResponse>> GetRecommendationHistoryAsync()
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var recommendationHistory = await _unitOfWork.Repository<DishRecommendHistory>()
                    .FindAsync(r => r.SmartDietUserId == userId,
                        include: query => query.Include(x => x.Dish)
                        .ThenInclude(x => x.DishIngredients));

                return _mapper.Map<IEnumerable<DishResponse>>(recommendationHistory.Select(r => r.Dish));
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Log the exception and rethrow or handle it as needed
                throw new ErrorException(StatusCodes.Status500InternalServerError,
                    ErrorCode.INTERNAL_SERVER_ERROR,
                    "Failed to get dish recommendation history");
            }
        }

        private async Task<double> CalculateDishScore(Dish dish, string userId)
        {
            try
            {
                // Calculate average rating
                var ratings = await _unitOfWork.Repository<DishRating>()
                    .FindAsync(dr => dr.DishId == dish.Id);
                
                var averageRating = ratings.Any() 
                    ? ratings.Average(dr => dr.Rating) 
                    : 0;

                var dishRatingPoints = averageRating * _settings.Points.DishRatingPerStar;

                // Get user interaction
                var userInteraction = await _unitOfWork.Repository<UserDishInteraction>()
                    .FirstOrDefaultAsync(udi => udi.SmartDietUserId == userId && udi.DishId == dish.Id);

                // Calculate interaction points
                var interactionPoints = userInteraction?.InteractionType switch
                {
                    InteractionType.Liked => _settings.Points.LikedDish,
                    InteractionType.Disliked => _settings.Points.DislikedDish,
                    _ => 0
                };

                // Add points for new dishes
                var newDishPoints = userInteraction == null ? _settings.Points.NewDish : 0;

                // Add seasonal bonus if applicable
                var seasonalBonus = IsSeasonalDish(dish) ? _settings.Points.SeasonalBonus : 0;

                return dishRatingPoints + interactionPoints + newDishPoints + seasonalBonus;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating score for dish {dish.Id}: {ex}");
                return 0;
            }
        }

        private bool IsSeasonalDish(Dish dish)
        {
            // Implement seasonal logic here
            return false;
        }

        // Implement other interface methods similarly to MealRecommendationService
        // RegenerateRecommendationsAsync
        // GetRecommendedDishesAsync
        // GetRecommendationHistoryAsync
    }
}