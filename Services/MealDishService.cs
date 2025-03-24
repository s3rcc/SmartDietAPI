using AutoMapper;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.MealDishDTOs;
using Microsoft.AspNetCore.Http;
using Repositories.Interfaces;
using Services.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class MealDishService : IMealDishService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public MealDishService(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task<IEnumerable<MealDishResponse>> CreateMealDishesAsync(IEnumerable<MealDishDTO> mealDishDTOs)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var mealDishes = new List<MealDish>();

                foreach (var mealDishDTO in mealDishDTOs)
                {
                    // Check if MealDish already exists
                    var existingMealDish = await _unitOfWork.Repository<MealDish>().FirstOrDefaultAsync(x => x.MealId == mealDishDTO.MealId && x.DishId == mealDishDTO.DishId);
                    if (existingMealDish != null)
                    {
                        throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, $"MealDish with MealId {mealDishDTO.MealId} and DishId {mealDishDTO.DishId} already exists");
                    }

                    // Create map
                    var mealDish = _mapper.Map<MealDish>(mealDishDTO);

                    // Set create time and user
                    mealDish.CreatedTime = DateTime.UtcNow;
                    mealDish.CreatedBy = userId;

                    mealDishes.Add(mealDish);
                }

                // Save MealDishes to the database
                await _unitOfWork.Repository<MealDish>().AddRangeAsync(mealDishes);
                await _unitOfWork.SaveChangeAsync();

                return mealDishes.Select(md => new MealDishResponse
                {
                    Id = md.Id,
                    MealId = md.MealId,
                    DishId = md.DishId,
                    ServingSize = md.ServingSize
                });
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

        public async Task DeleteMealDishAsync(string mealDishId)
        {
            try
            {
                // Retrieve existing MealDish
                var existingMealDish = await _unitOfWork.Repository<MealDish>().GetByIdAsync(mealDishId)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "MealDish does not exist!");

                // Hard delete MealDish from the database
                _unitOfWork.Repository<MealDish>().DeleteAsync(existingMealDish);
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

        public async Task<IEnumerable<MealDishResponse>> GetMealDishesByMealIdAsync(string mealId)
        {
            try
            {
                var mealDishes = await _unitOfWork.Repository<MealDish>().FindAsync(
                    x => x.MealId == mealId,
                    include: query => query.Include(x => x.Dish),
                    orderBy: query => query.OrderByDescending(x => x.CreatedTime)
                );

                return mealDishes.Select(md => new MealDishResponse
                {
                    Id = md.Id,
                    MealId = md.MealId,
                    DishId = md.DishId,
                    ServingSize = md.ServingSize
                });
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