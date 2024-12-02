using AutoMapper;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.FavoriteMealDTOs;
using DTOs.FoodDTOs;
using Microsoft.AspNetCore.Http;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class FavoriteMealService : IFavoriteMealService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public FavoriteMealService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task CreateFavoriteMealAsync(FavoriteMealDTO favoriteMealDTO)
        {
            try
            {
                var existingFavoriteMeal = await _unitOfWork.Repository<FavoriteMeal>().FindAsync(
                    x => x.SmartDietUserId == favoriteMealDTO.SmartDietUserId && x.MealId == favoriteMealDTO.MealId);

                if (existingFavoriteMeal != null)
                    throw new ErrorException(
                        StatusCodes.Status400BadRequest, 
                        ErrorCode.BADREQUEST, 
                        "Favorite meal already exists!");

                var favoriteMeal = _mapper.Map<FavoriteMeal>(favoriteMealDTO);
                favoriteMeal.CreatedTime = DateTime.UtcNow;
                favoriteMeal.CreatedBy = "system";  
                await _unitOfWork.Repository<FavoriteMeal>().AddAsync(favoriteMeal);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        //Hard Delete
        public async Task DeleteFavoriteMealAsync(string favoriteMealId)
        {
            try
            {
                var favoriteMeal = await _unitOfWork.Repository<FavoriteMeal>().GetByIdAsync(favoriteMealId)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Favorite meal does not exist!");

                //favoriteMeal.DeletedTime = DateTime.UtcNow;
                _unitOfWork.Repository<FavoriteMeal>().DeleteAsync(favoriteMeal);
                await _unitOfWork.SaveChangeAsync();
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
                var favoriteMeals = await _unitOfWork.Repository<FavoriteMeal>().GetAllAsync(
                    includes: x => x.Meal);

                return _mapper.Map<IEnumerable<FavoriteMealResponse>>(favoriteMeals);
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
                var favoriteMeals = await _unitOfWork.Repository<FavoriteMeal>().GetAllWithPaginationAsync(
                    pageIndex,
                    pageSize,
                    includes: x => x.Meal,
                    searchTerm: x => string.IsNullOrEmpty(searchTerm) || x.Meal.Name.Contains(searchTerm),
                    orderBy: x => x.OrderBy(f => f.Meal.Name));

                if (favoriteMeals == null || !favoriteMeals.Items.Any())
                    return new BasePaginatedList<FavoriteMealResponse>(new List<FavoriteMealResponse>(), 0, pageIndex, pageSize);

                var favoriteMealResponses = _mapper.Map<IEnumerable<FavoriteMealResponse>>(favoriteMeals.Items);

                var rs = favoriteMealResponses.ToList();
                return new BasePaginatedList<FavoriteMealResponse>(
                    rs,
                    rs.Count,
                    pageIndex,
                    pageSize);
            }
            catch (AutoMapperMappingException ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, $"AutoMapper error: {ex.Message}");
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
                var favoriteMeal = await _unitOfWork
                    .Repository<FavoriteMeal>()
                    .GetByIdAsync(
                        id,
                        includes: x => x.Meal)
                    ?? throw new ErrorException(
                        StatusCodes.Status404NotFound, 
                        ErrorCode.NOT_FOUND, 
                        "Favorite meal does not exist!");

                return _mapper.Map<FavoriteMealResponse>(favoriteMeal);
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
                var favoriteMeal = await _unitOfWork
                    .Repository<FavoriteMeal>()
                    .GetByIdAsync(favoriteMealId)
                    ?? throw new ErrorException(
                        StatusCodes.Status404NotFound, 
                        ErrorCode.NOT_FOUND, 
                        "Favorite meal does not exist!");

                _mapper.Map(favoriteMealDTO, favoriteMeal);
                favoriteMeal.LastUpdatedTime = DateTime.UtcNow;
                await _unitOfWork.Repository<FavoriteMeal>().UpdateAsync(favoriteMeal);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }
    }

}
