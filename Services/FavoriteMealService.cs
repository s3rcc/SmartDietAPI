using AutoMapper;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.FavoriteMealDTOs;
using Microsoft.AspNetCore.Http;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class FavoriteMealService : IFavoriteMealService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public FavoriteMealService(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task CreateFavoriteMealAsync(FavoriteMealDTO favoriteMealDTO)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                // Check if the favorite meal already exists for the user
                var existingFavoriteMeal = await _unitOfWork.Repository<FavoriteMeal>().FindAsync(
                    x => x.SmartDietUserId == userId && x.MealId == favoriteMealDTO.MealId);

                if (existingFavoriteMeal.Any())
                {
                    throw new ErrorException(
                        StatusCodes.Status400BadRequest,
                        ErrorCode.BADREQUEST,
                        "Favorite meal already exists for this user!");
                }

                var favoriteMeal = _mapper.Map<FavoriteMeal>(favoriteMealDTO);
                favoriteMeal.SmartDietUserId = userId;
                favoriteMeal.CreatedTime = DateTime.UtcNow;
                favoriteMeal.CreatedBy = userId; // Set the user ID from the token
                await _unitOfWork.Repository<FavoriteMeal>().AddAsync(favoriteMeal);
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

        // Hard Delete
        public async Task DeleteFavoriteMealAsync(string favoriteMealId)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                var favoriteMeal = await _unitOfWork.Repository<FavoriteMeal>().GetByIdAsync(favoriteMealId)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Favorite meal does not exist!");

                // Ensure the user is deleting their own favorite meal
                if (favoriteMeal.CreatedBy != userId)
                {
                    throw new ErrorException(
                        StatusCodes.Status403Forbidden,
                        ErrorCode.FORBIDDEN,
                        "You are not authorized to delete this favorite meal!");
                }

                _unitOfWork.Repository<FavoriteMeal>().DeleteAsync(favoriteMeal);
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

        public async Task<IEnumerable<FavoriteMealResponse>> GetAllFavoriteMealsAsync()
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                // Filter favorite meals by the logged-in user
                var favoriteMeals = await _unitOfWork.Repository<FavoriteMeal>().FindAsync(
                    x => x.CreatedBy == userId,
                    includes: x => x.Meal);

                return _mapper.Map<IEnumerable<FavoriteMealResponse>>(favoriteMeals);
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

        public async Task<BasePaginatedList<FavoriteMealResponse>> GetAllFavoriteMealsAsync(int pageIndex, int pageSize, string? searchTerm)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                // Filter favorite meals by the logged-in user and apply pagination
                var favoriteMeals = await _unitOfWork.Repository<FavoriteMeal>().FindWithPaginationAsync(
                    pageIndex,
                    pageSize,
                    predicate: x => x.CreatedBy == userId,
                    includes: x => x.Meal,
                    searchTerm: x => string.IsNullOrEmpty(searchTerm) || x.Meal.Name.Contains(searchTerm),
                    orderBy: x => x.OrderBy(f => f.Meal.Name)
                    ); // Add filter for the logged-in user

                if (favoriteMeals == null || !favoriteMeals.Items.Any())
                {
                    return new BasePaginatedList<FavoriteMealResponse>(new List<FavoriteMealResponse>(), 0, pageIndex, pageSize);
                }

                var favoriteMealResponses = _mapper.Map<IEnumerable<FavoriteMealResponse>>(favoriteMeals.Items);

                return new BasePaginatedList<FavoriteMealResponse>(
                    favoriteMealResponses.ToList(),
                    favoriteMeals.TotalItems,
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
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task<FavoriteMealResponse> GetFavoriteMealByIdAsync(string id)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                var favoriteMeal = await _unitOfWork
                    .Repository<FavoriteMeal>()
                    .GetByIdAsync(
                        id,
                        includes: x => x.Meal)
                    ?? throw new ErrorException(
                        StatusCodes.Status404NotFound,
                        ErrorCode.NOT_FOUND,
                        "Favorite meal does not exist!");

                // Ensure the user is accessing their own favorite meal
                if (favoriteMeal.CreatedBy != userId)
                {
                    throw new ErrorException(
                        StatusCodes.Status403Forbidden,
                        ErrorCode.FORBIDDEN,
                        "You are not authorized to access this favorite meal!");
                }

                return _mapper.Map<FavoriteMealResponse>(favoriteMeal);
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

        public async Task UpdateFavoriteMealAsync(string favoriteMealId, FavoriteMealDTO favoriteMealDTO)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                var favoriteMeal = await _unitOfWork
                    .Repository<FavoriteMeal>()
                    .GetByIdAsync(favoriteMealId)
                    ?? throw new ErrorException(
                        StatusCodes.Status404NotFound,
                        ErrorCode.NOT_FOUND,
                        "Favorite meal does not exist!");

                // Ensure the user is updating their own favorite meal
                if (favoriteMeal.CreatedBy != userId)
                {
                    throw new ErrorException(
                        StatusCodes.Status403Forbidden,
                        ErrorCode.FORBIDDEN,
                        "You are not authorized to update this favorite meal!");
                }

                _mapper.Map(favoriteMealDTO, favoriteMeal);
                favoriteMeal.LastUpdatedTime = DateTime.UtcNow;
                favoriteMeal.LastUpdatedBy = userId; // Set the user ID from the token
                await _unitOfWork.Repository<FavoriteMeal>().UpdateAsync(favoriteMeal);
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