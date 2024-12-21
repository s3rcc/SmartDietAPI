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
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class DishService : IDishService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;
        public DishService(IUnitOfWork unitOfWork, IMapper mapper, ICloudinaryService cloudinaryService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
        }
        public async Task CreateDishAsync(DishDTO dishDTO, List<DishIngredientDTO> dishIngredientDTOs)
        {
            try
            {
                // Check if dish name already exist
                var existingName = await _unitOfWork.Repository<Dish>().FirstOrDefaultAsync(x => x.Name == dishDTO.Name);
                if(existingName != null)
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Dish name already exist");
                }
                // Create map
                var dish = _mapper.Map<Dish>(dishDTO);
                // Process image
                if(dishDTO.Image != null)
                {
                    dish.Image = await _cloudinaryService.UploadImageAsync(dishDTO.Image);
                }

                // Set created time
                dish.CreatedTime = DateTime.Now;
                dish.CreatedBy = "System";
                // Save change to the database
                await _unitOfWork.Repository<Dish>().AddAsync(dish);
                await _unitOfWork.SaveChangeAsync();

                //process food ingredients
                if(dishIngredientDTOs != null && dishIngredientDTOs.Any())
                {
                    var dishIngredients = dishIngredientDTOs.Select(
                        ingredient => new DishIngredient
                        {
                            DishId = dish.Id,
                            FoodId = ingredient.FoodId,
                            Quantity = ingredient.Quantity,
                            CreatedTime = DateTime.Now,
                            CreatedBy = "System"
                        }).ToList();
                    // Save ingredient to the database
                    try
                    {
                    await _unitOfWork.Repository<DishIngredient>().AddRangeAsync(dishIngredients);
                    await _unitOfWork.SaveChangeAsync();
                    }
                    catch
                    {
                        _unitOfWork.Repository<Dish>().DeleteAsync(dish);
                        await _unitOfWork.SaveChangeAsync();
                        throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "Error while saving dish ingredient");
                        
                    }
                }
                
            }
            catch
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "An error occured");
            }
        }

        public async Task DeleteDishAsync(string dishId)
        {
            // retrive existing dish
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

            // retrive dish ingredient
            var dishIngredient = existingDish.DishIngredients;
            // Delete dish
            existingDish.DeletedTime = DateTime.UtcNow;
            existingDish.DeletedBy = "System";
            // Set `DeletedTime` and `DeletedBy` for each ingredient
            foreach (var ingredient in existingDish.DishIngredients)
            {
                ingredient.DeletedTime = DateTime.UtcNow;
                ingredient.DeletedBy = "System";
            }
            // Saving change to 2 table
            await _unitOfWork.Repository<Dish>().UpdateAsync(existingDish);
            await _unitOfWork.Repository<DishIngredient>().UpdateRangeAsync(existingDish.DishIngredients);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task<IEnumerable<DishResponse>> GetAllDishesAsync()
        {
            try
            {
            var dishes = await _unitOfWork.Repository<Dish>().GetAllAsync(
                includes: x => x.DishIngredients);
            return _mapper.Map<IEnumerable<DishResponse>>(dishes);

            }
            catch
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "An error occured");
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
                if(dishes == null || !dishes.Items.Any())
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
            catch
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "An error occured");
            }
        }

        public async Task<DishResponse> GetDishByIdAsync(string dishId)
        {
            try
            {
                var dish = await _unitOfWork.Repository<Dish>().GetByIdAsync(
                    dishId,
                    includes: x => x.DishIngredients)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Dish does not exist!"); ;
                return _mapper.Map<DishResponse>(dish);
            }
            catch
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "An error occured");
            }
        }

        public async Task UpdateDishAsync(string dishId, DishDTO dishDTO, List<DishIngredientDTO> dishIngredientDTOs)
        {
            try
            {
                // Retrive eixsting dish
                var existingDish = await _unitOfWork.Repository<Dish>().GetByIdAsync(
                    dishId,
                    includes: x => x.DishIngredients)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Dish does not exist!");
                // Check if dish name exist
                var existingName = await _unitOfWork.Repository<Dish>().FirstOrDefaultAsync(x => x.Name == dishDTO.Name && x.Id != dishId);
                if (existingName != null)
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Dish name already exist");
                }
                // Retrive old image
                var oldImgUrl = existingDish.Image;
                // Map dish
                var dish = _mapper.Map<Dish>(dishDTO);
                // Process image of dish
                if(dishDTO.Image != null)
                {
                    dish.Image = await _cloudinaryService.UploadImageAsync(dishDTO.Image);
                    if (!string.IsNullOrEmpty(oldImgUrl))
                    {
                        var publicId = oldImgUrl.Split('/').Last().Split('.')[0];
                        // Delete old image
                        await _cloudinaryService.DeleteImageAsync(publicId);
                    }
                }
                else
                {
                    // Keep old image
                    dish.Image = oldImgUrl;
                }
                // Handle dish ingredient
                // Remove existing dish ingredients
                if (existingDish.DishIngredients != null && existingDish.DishIngredients.Any())
                {
                    _unitOfWork.Repository<DishIngredient>().DeleteRangeAsync(existingDish.DishIngredients);
                    await _unitOfWork.SaveChangeAsync();
                }

                // Add new dish ingredients
                if(dishIngredientDTOs != null && dishIngredientDTOs.Any())
                {
                    var newIngredients = dishIngredientDTOs.Select
                        (ingredient => new DishIngredient
                        {
                            DishId = existingDish.Id,
                            FoodId = ingredient.FoodId,
                            Quantity = ingredient.Quantity,
                            CreatedTime = DateTime.UtcNow,
                            CreatedBy = "System"
                        }).ToList();

                    // Save to DB
                        await _unitOfWork.Repository<DishIngredient>().AddRangeAsync(newIngredients);
                        await _unitOfWork.SaveChangeAsync();
                }

                // Set update time
                dish.LastUpdatedTime = DateTime.UtcNow;
                dish.LastUpdatedBy = "System";
                await _unitOfWork.Repository<Dish>().UpdateAsync(dish);
                await _unitOfWork.SaveChangeAsync();
            }
            catch
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "An error occured");
            }
        }
    }
}
