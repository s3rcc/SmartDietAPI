using AutoMapper;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.MealDTOs;
using Microsoft.AspNetCore.Http;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class MealService : IMealService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public MealService(IUnitOfWork unitOfWork, ICloudinaryService cloudinaryService, IMapper mapper, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task CreateMealAsync(MealDTO mealDTO)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                // Check if meal name exists
                var existingMeal = await _unitOfWork.Repository<Meal>().FirstOrDefaultAsync(x => x.Name == mealDTO.Name);
                if (existingMeal != null)
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Meal name already exists");
                }

                // Create map
                var meal = _mapper.Map<Meal>(mealDTO);

                // Process image
                if (mealDTO.Image != null)
                {
                    meal.Image = await _cloudinaryService.UploadImageAsync(mealDTO.Image);
                }

                // Set create time and user
                meal.CreatedTime = DateTime.UtcNow;
                meal.CreatedBy = userId;

                await _unitOfWork.Repository<Meal>().AddAsync(meal);
                await _unitOfWork.SaveChangeAsync();

                // Handle Meal Dishes
                if (mealDTO.DishIds != null && mealDTO.DishIds.Any())
                {
                    var mealDishes = mealDTO.DishIds.Select(dishId => new MealDish
                    {
                        MealId = meal.Id,
                        DishId = dishId,
                        CreatedTime = DateTime.UtcNow,
                        CreatedBy = userId
                    }).ToList();

                    // Add meal dishes
                    await _unitOfWork.Repository<MealDish>().AddRangeAsync(mealDishes);
                    await _unitOfWork.SaveChangeAsync();
                }
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task DeleteMealAsync(string id)
        {
            try
            {
                var meal = await _unitOfWork.Repository<Meal>().GetByIdAsync(id)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Meal does not exist!");

                // Check if meal is favorited
                var favMeal = await _unitOfWork.Repository<FavoriteMeal>().FindAsync(x => x.MealId == id);
                if (favMeal.Any())
                {
                    throw new ErrorException(StatusCodes.Status409Conflict, ErrorCode.CONFLICT, "Someone has this meal in their favorite!");
                }

                meal.DeletedTime = DateTime.UtcNow;
                meal.DeletedBy = _tokenService.GetUserIdFromToken();

                await _unitOfWork.Repository<Meal>().UpdateAsync(meal);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task<IEnumerable<MealResponse>> GetAllMealsAsync()
        {
            try
            {
                var meals = await _unitOfWork.Repository<Meal>().GetAllAsync(
                    includes: [
                        x => x.MealDishes,
                        x => x.MealDishes.Select(md => md.Dish)
                    ]);

                return _mapper.Map<IEnumerable<MealResponse>>(meals);
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task<BasePaginatedList<MealResponse>> GetAllMealsAsync(int pageIndex, int pageSize, string? searchTerm)
        {
            try
            {
                BasePaginatedList<Meal> meals = await _unitOfWork.Repository<Meal>().GetAllWithPaginationAsync(
                    pageIndex,
                    pageSize,
                    includes: x => x.MealDishes,
                    searchTerm: x => string.IsNullOrEmpty(searchTerm) || x.Name.Contains(searchTerm),
                    orderBy: x => x.OrderBy(f => f.Name)
                );

                if (meals == null || !meals.Items.Any())
                {
                    return new BasePaginatedList<MealResponse>(new List<MealResponse>(), 0, pageIndex, pageSize);
                }

                var mealResponses = _mapper.Map<List<MealResponse>>(meals.Items);
                return new BasePaginatedList<MealResponse>(
                    mealResponses,
                    mealResponses.Count,
                    pageIndex,
                    pageSize);
            }
            catch (AutoMapperMappingException ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, $"AutoMapper error: {ex.Message}");
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, $"An error occurred: {ex.Message}");
            }
        }

        public async Task<MealResponse> GetMealByIdAsync(string id)
        {
            try
            {
                var meal = await _unitOfWork.Repository<Meal>().GetByIdAsync(
                    id,
                    includes: x => x.MealDishes)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Meal does not exist!");

                return _mapper.Map<MealResponse>(meal);
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task UpdateMealAsync(string id, MealDTO mealDTO)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                // Check if meal exists
                var existingMeal = await _unitOfWork.Repository<Meal>().GetByIdAsync(id)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Meal does not exist!");

                // Check if name exists and is not the current meal
                var existingName = await _unitOfWork.Repository<Meal>().FirstOrDefaultAsync(x => x.Name == mealDTO.Name && x.Id != id);
                if (existingName != null)
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Meal name already exists");
                }

                // Retrieve old image
                var oldImgUrl = existingMeal.Image;

                var meal = _mapper.Map<Meal>(mealDTO);

                // Process image
                if (mealDTO.Image != null)
                {
                    // Upload new image
                    meal.Image = await _cloudinaryService.UploadImageAsync(mealDTO.Image);

                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(oldImgUrl))
                    {
                        var publicId = oldImgUrl.Split('/').Last().Split('.')[0];
                        await _cloudinaryService.DeleteImageAsync(publicId);
                    }
                }
                else
                {
                    // Keep old image
                    meal.Image = oldImgUrl;
                }

                // Handle Meal Dishes
                // First, remove existing dishes
                if (existingMeal.MealDishes != null && existingMeal.MealDishes.Any())
                {
                    _unitOfWork.Repository<MealDish>().DeleteRangeAsync(existingMeal.MealDishes);
                    await _unitOfWork.SaveChangeAsync();
                }

                // Add new dishes if provided
                if (mealDTO.DishIds != null && mealDTO.DishIds.Any())
                {
                    var newMealDishes = mealDTO.DishIds.Select(dishId => new MealDish
                    {
                        MealId = id,
                        DishId = dishId,
                        CreatedTime = DateTime.UtcNow,
                        CreatedBy = userId
                    }).ToList();

                    // Add new meal dishes
                    await _unitOfWork.Repository<MealDish>().AddRangeAsync(newMealDishes);
                    await _unitOfWork.SaveChangeAsync();
                }

                // Set last updated time and user
                meal.LastUpdatedTime = DateTime.UtcNow;
                meal.LastUpdatedBy = userId;

                await _unitOfWork.Repository<Meal>().UpdateAsync(meal);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }
    }
}