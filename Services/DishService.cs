using AutoMapper;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.DishDTOs;
using Microsoft.AspNetCore.Http;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class DishService : IDishService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ITokenService _tokenService;

        public DishService(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryService cloudinaryService, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
            _tokenService = tokenService;
        }

        public async Task CreateDishAsync(DishDTO dishDTO, List<DishIngredientDTO> dishIngredientDTOs)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                // Check if dish name already exists
                var existingName = await _unitOfWork.Repository<Dish>().FirstOrDefaultAsync(x => x.Name == dishDTO.Name);
                if (existingName != null)
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Dish name already exists");
                }

                // Create map
                var dish = _mapper.Map<Dish>(dishDTO);

                // Process image
                if (dishDTO.Image != null)
                {
                    dish.Image = await _cloudinaryService.UploadImageAsync(dishDTO.Image);
                }

                // Set created time and user
                dish.CreatedTime = DateTime.UtcNow;
                dish.CreatedBy = userId;

                // Save dish to the database
                await _unitOfWork.Repository<Dish>().AddAsync(dish);
                await _unitOfWork.SaveChangeAsync();

                // Process dish ingredients
                if (dishIngredientDTOs != null && dishIngredientDTOs.Any())
                {
                    var dishIngredients = dishIngredientDTOs.Select(
                        ingredient => new DishIngredient
                        {
                            DishId = dish.Id,
                            FoodId = ingredient.FoodId,
                            Quantity = ingredient.Quantity,
                            CreatedTime = DateTime.UtcNow,
                            CreatedBy = userId
                        }).ToList();

                    // Save ingredients to the database
                    try
                    {
                        await _unitOfWork.Repository<DishIngredient>().AddRangeAsync(dishIngredients);
                        await _unitOfWork.SaveChangeAsync();
                    }
                    catch
                    {
                        // Rollback dish creation if ingredient saving fails
                        _unitOfWork.Repository<Dish>().DeleteAsync(dish);
                        await _unitOfWork.SaveChangeAsync();
                        throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "Error while saving dish ingredients");
                    }
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

        public async Task DeleteDishAsync(string dishId)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                // Retrieve existing dish
                var existingDish = await _unitOfWork.Repository<Dish>().GetByIdAsync(dishId,
                    includes: x => x.DishIngredients)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Dish does not exist");

                // Check if dish is in any meal
                var meal = await _unitOfWork.Repository<MealDish>().FindAsync(x => x.DishId == dishId);
                if (meal.Any())
                {
                    throw new ErrorException(StatusCodes.Status409Conflict, ErrorCode.CONFLICT, "Dish is in one or more meals!");
                }

                // Check if dish is favorited
                var favDish = await _unitOfWork.Repository<FavoriteDish>().FindAsync(x => x.DishId == dishId);
                if (favDish.Any())
                {
                    throw new ErrorException(StatusCodes.Status409Conflict, ErrorCode.CONFLICT, "Someone has this dish in their favorite!");
                }

                // Set deleted time and user
                existingDish.DeletedTime = DateTime.UtcNow;
                existingDish.DeletedBy = userId;

                // Set `DeletedTime` and `DeletedBy` for each ingredient
                foreach (var ingredient in existingDish.DishIngredients)
                {
                    ingredient.DeletedTime = DateTime.UtcNow;
                    ingredient.DeletedBy = userId;
                }

                // Save changes to the database
                await _unitOfWork.Repository<Dish>().UpdateAsync(existingDish);
                await _unitOfWork.Repository<DishIngredient>().UpdateRangeAsync(existingDish.DishIngredients);
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

        public async Task<IEnumerable<DishResponse>> GetAllDishesAsync()
        {
            try
            {
                var dishes = await _unitOfWork.Repository<Dish>().GetAllAsync(
                    includes: x => x.DishIngredients);

                return _mapper.Map<IEnumerable<DishResponse>>(dishes);
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

        public async Task<BasePaginatedList<DishResponse>> GetAllDishesAsync(int pageIndex, int pageSize, string? searchTerm)
        {
            try
            {
                BasePaginatedList<Dish> dishes = await _unitOfWork.Repository<Dish>().GetAllWithPaginationAsync(
                    pageIndex,
                    pageSize,
                    includes: x => x.DishIngredients,
                    searchTerm: x => string.IsNullOrEmpty(searchTerm) || x.Name.Contains(searchTerm),
                    orderBy: x => x.OrderBy(d => d.Name));

                if (dishes == null || !dishes.Items.Any())
                {
                    return new BasePaginatedList<DishResponse>(new List<DishResponse>(), 0, pageIndex, pageSize);
                }

                var dishResponses = _mapper.Map<List<DishResponse>>(dishes.Items);
                return new BasePaginatedList<DishResponse>(
                    dishResponses,
                    dishResponses.Count,
                    pageIndex,
                    pageSize);
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

        public async Task<DishResponse> GetDishByIdAsync(string dishId)
        {
            try
            {
                var dish = await _unitOfWork.Repository<Dish>().GetByIdAsync(
                    dishId,
                    includes: x => x.DishIngredients)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Dish does not exist!");

                return _mapper.Map<DishResponse>(dish);
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

        public async Task UpdateDishAsync(string dishId, DishDTO dishDTO, List<DishIngredientDTO> dishIngredientDTOs)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                // Retrieve existing dish
                var existingDish = await _unitOfWork.Repository<Dish>().GetByIdAsync(
                    dishId,
                    includes: x => x.DishIngredients)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Dish does not exist!");

                // Check if dish name exists and is not the current dish
                var existingName = await _unitOfWork.Repository<Dish>().FirstOrDefaultAsync(x => x.Name == dishDTO.Name && x.Id != dishId);
                if (existingName != null)
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Dish name already exists");
                }

                // Retrieve old image
                var oldImgUrl = existingDish.Image;

                // Map dish
                var dish = _mapper.Map<Dish>(dishDTO);

                // Process image
                if (dishDTO.Image != null)
                {
                    dish.Image = await _cloudinaryService.UploadImageAsync(dishDTO.Image);

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
                    dish.Image = oldImgUrl;
                }

                // Handle dish ingredients
                // Remove existing dish ingredients
                if (existingDish.DishIngredients != null && existingDish.DishIngredients.Any())
                {
                    _unitOfWork.Repository<DishIngredient>().DeleteRangeAsync(existingDish.DishIngredients);
                    await _unitOfWork.SaveChangeAsync();
                }

                // Add new dish ingredients
                if (dishIngredientDTOs != null && dishIngredientDTOs.Any())
                {
                    var newIngredients = dishIngredientDTOs.Select(
                        ingredient => new DishIngredient
                        {
                            DishId = existingDish.Id,
                            FoodId = ingredient.FoodId,
                            Quantity = ingredient.Quantity,
                            CreatedTime = DateTime.UtcNow,
                            CreatedBy = userId
                        }).ToList();

                    // Save new ingredients to the database
                    await _unitOfWork.Repository<DishIngredient>().AddRangeAsync(newIngredients);
                    await _unitOfWork.SaveChangeAsync();
                }

                // Set last updated time and user
                dish.LastUpdatedTime = DateTime.UtcNow;
                dish.LastUpdatedBy = userId;

                // Save changes to the database
                await _unitOfWork.Repository<Dish>().UpdateAsync(dish);
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