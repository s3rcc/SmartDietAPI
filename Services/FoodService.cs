using AutoMapper;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.FoodDTOs;
using Microsoft.AspNetCore.Http;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class FoodService : IFoodService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IMapper _mapper;
        public FoodService(IUnitOfWork unitOfWork, ICloudinaryService cloudinaryService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
            _mapper = mapper;
        }
        public async Task CreateFoodAsync(FoodDTO foodDTO)
        {
            try
            {
                // Check if food name exist
                var existingFood = await _unitOfWork.Repository<Food>().FirstOrDefaultAsync(x => x.Name == foodDTO.Name);
                if (existingFood != null)
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Food name already exist");
                };
                // Create map
                var food = _mapper.Map<Food>(foodDTO);
                // Process image
                if(foodDTO.Image != null)
                {
                    food.Image = await _cloudinaryService.UploadImageAsync(foodDTO.Image);
                }

                // Set create time
                food.CreatedTime = DateTime.UtcNow;
                food.CreatedBy = "system";
                await _unitOfWork.Repository<Food>().AddAsync(food);
                await _unitOfWork.SaveChangeAsync();

                // Handle Food Allergies
                if (foodDTO.AllergenFoodIds != null && foodDTO.AllergenFoodIds.Any())
                {
                    var foodAllergies = foodDTO.AllergenFoodIds.Select(allergenId => new FoodAllergy
                    {
                        FoodId = food.Id,
                        AllergenFoodId = allergenId,
                        CreatedTime = DateTime.UtcNow,
                        CreatedBy = "System"
                    }).ToList();

                    // Add food allergies
                    await _unitOfWork.Repository<FoodAllergy>().AddRangeAsync(foodAllergies);
                    await _unitOfWork.SaveChangeAsync();
                }
            }
            catch
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "An error occured");

            }
        }

        public async Task DeleteFoodAsync(string foodId)
        {
            try
            {
                var food = await _unitOfWork.Repository<Food>().GetByIdAsync(foodId)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Food does not exist!");
                // Check if food is in dish
                var dish = await _unitOfWork.Repository<DishIngredient>().FindAsync(x => x.FoodId == foodId);
                if (dish.Any())
                {
                    throw new ErrorException(StatusCodes.Status409Conflict, ErrorCode.CONFLICT, "Food is in one or more dishes!");
                }

                // Check if food is in fridge
                var fridge = await _unitOfWork.Repository<FridgeItem>().FindAsync(x => x.FoodId == foodId);
                if (fridge.Any())
                {
                    throw new ErrorException(StatusCodes.Status409Conflict, ErrorCode.CONFLICT, "Food is in someone fridge!");
                }

                food.DeletedTime = DateTime.UtcNow;
                //food.DeletedBy = Holder;
                await _unitOfWork.Repository<Food>().UpdateAsync(food);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task<IEnumerable<FoodResponse>> GetAllFoodsAsync()
        {
            try
            {
                var foods = await _unitOfWork.Repository<Food>().GetAllAsync(
                    includes: [
                    x => x.FoodAllergies,
                    x => x.NutrientCategories
                ]
                );
                return _mapper.Map<IEnumerable<FoodResponse>>(foods);
            }
            catch
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "An error occured");
            }
        }

        public async Task<BasePaginatedList<FoodResponse>> GetAllFoodsAsync(int pageIndex, int pageSize, string? searchTearm)
        {
            try
            {
                BasePaginatedList<Food> foods = await _unitOfWork.Repository<Food>().GetAllWithPaginationAsync(
                    pageIndex,
                    pageSize,
                    includes: [
                        x => x.FoodAllergies,
                        x => x.NutrientCategories
                        ],
                    searchTerm: x => string.IsNullOrEmpty(searchTearm) || x.Name.Contains(searchTearm),
                    orderBy: x => x.OrderBy(f => f.Name)
                    );
                if (foods == null || !foods.Items.Any())
                {
                    return new BasePaginatedList<FoodResponse>(new List<FoodResponse>(), 0, pageIndex, pageSize);
                }
                var foodResponses = _mapper.Map<List<FoodResponse>>(foods.Items);
                return new BasePaginatedList<FoodResponse>(
                    foodResponses, 
                    foodResponses.Count, 
                    pageIndex, 
                    pageSize);
            }
            catch (AutoMapperMappingException ex)
            {
                // Log or inspect the exception for mapping errors
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR,$"AutoMapper error: {ex.Message}");
                // Optionally log the stack trace or inner exceptions
            }
            catch
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "An error occured");
            }
        }

        public async Task<FoodResponse> GetDeletedFoodByIdAsync(string id)
        {
            try
            {
                var food = await _unitOfWork.Repository<Food>().GetDeletedByIdAsync(
                    id,
                    includes: [
                        x => x.FoodAllergies,
                        x => x.NutrientCategories
                        ]);
                return _mapper.Map<FoodResponse>(food);
            }
            catch
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "An error occured");
            }
        }

        public async Task<IEnumerable<FoodResponse>> GetDeletedFoodsAsync()
        {
            try
            {
                var foods = await _unitOfWork.Repository<Food>().GetAllDeletedAsync(
                    includes: [
                        x => x.FoodAllergies,
                        x => x.NutrientCategories
                        ]);
                return _mapper.Map<IEnumerable<FoodResponse>>(foods);
            }
            catch
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "An error occured");
            }
        }

        public async Task<FoodResponse> GetFoodByIdAsync(string id)
        {
            try
            {
                var food = await _unitOfWork.Repository<Food>().GetByIdAsync(
                    id,
                    includes: [
                        x => x.FoodAllergies,
                        x => x.NutrientCategories
                        ])
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Food does not exist!");
                return _mapper.Map<FoodResponse>(food);
            }
            catch
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "An error occured");
            }
        }

        public async Task UpdateFoodAsync(string foodId,FoodDTO foodDTO)
        {
            try
            {
                // Check if food exist
                var existingFood = await _unitOfWork.Repository<Food>().GetByIdAsync(foodId)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Food does not exist!");
                // Check if name and is not the current food exist
                var existingName = await _unitOfWork.Repository<Food>().FirstOrDefaultAsync(x => x.Name == foodDTO.Name && x.Id != foodId);
                if(existingName != null)
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Food name already exist");
                }
                // Retrive old image
                var oldImgUrl = existingFood.Image;

                var food = _mapper.Map<Food>(foodDTO);
                if (foodDTO.Image != null)
                {
                    // Upload new image
                    food.Image = await _cloudinaryService.UploadImageAsync(foodDTO.Image);

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
                    food.Image = oldImgUrl;
                }

                // Handle Food Allergies
                // First, remove existing allergies
                if (existingFood.FoodAllergies != null && existingFood.FoodAllergies.Any())
                {
                    _unitOfWork.Repository<FoodAllergy>().DeleteRangeAsync(existingFood.FoodAllergies);
                    await _unitOfWork.SaveChangeAsync();
                }

                // Add new allergies if provided
                if (foodDTO.AllergenFoodIds != null && foodDTO.AllergenFoodIds.Any())
                {
                    var newAllergies = foodDTO.AllergenFoodIds.Select(allergenId => new FoodAllergy
                    {
                        FoodId = foodId,
                        AllergenFoodId = allergenId,
                        CreatedTime = DateTime.UtcNow,
                        CreatedBy = "System"
                    }).ToList();

                    // Add new food allergies
                    await _unitOfWork.Repository<FoodAllergy>().AddRangeAsync(newAllergies);
                    await _unitOfWork.SaveChangeAsync();
                }

                // Set last updated time
                food.LastUpdatedTime = DateTime.UtcNow;
                //food.LastUpdatedBy = SmartDietUserId;
                await _unitOfWork.Repository<Food>().UpdateAsync(food);
                await _unitOfWork.SaveChangeAsync();

            }
            catch
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "An error occured");
            }
        }

    }
}
