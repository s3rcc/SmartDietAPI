using AutoMapper;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.FavoriteDishDTOs;
using DTOs.FavoriteMealDTOs;
using Microsoft.AspNetCore.Http;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class FavoriteDishService : IFavoriteDishService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public FavoriteDishService(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task<FavoriteDishResponse> GetFavoriteDishByIdAsync(string id)
        {
            try
            {
                var favoriteDish = await _unitOfWork.Repository<FavoriteDish>().GetByIdAsync(id, includes: x => x.Dish)
                                  ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Favorite Dish not found!");
                return _mapper.Map<FavoriteDishResponse>(favoriteDish);
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

        public async Task<IEnumerable<FavoriteDishResponse>> GetAllFavoriteDishesAsync()
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var favoriteDishes = await _unitOfWork
                    .Repository<FavoriteDish>()
                    .FindAsync(
                    x => x.CreatedBy == userId,
                    includes: x => x.Dish);
                return _mapper.Map<IEnumerable<FavoriteDishResponse>>(favoriteDishes);
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

        public async Task<BasePaginatedList<FavoriteDishResponse>> GetAllFavoriteDishesAsync(int pageIndex, int pageSize, string? searchTerm)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var favoriteDishes = await _unitOfWork.Repository<FavoriteDish>().GetAllWithPaginationAsync(
                    pageIndex,
                    pageSize,
                    includes: x => x.Dish,
                    searchTerm: x => string.IsNullOrEmpty(searchTerm) || x.Dish.Name.Contains(searchTerm),
                    orderBy: x => x.OrderBy(d => d.Dish.Name)
                );

                if (favoriteDishes == null || !favoriteDishes.Items.Any())
                {
                    return new BasePaginatedList<FavoriteDishResponse>(
                        new List<FavoriteDishResponse>(),
                        0,
                        pageIndex,
                        pageSize);
                }

                var response = _mapper.Map<List<FavoriteDishResponse>>(favoriteDishes.Items);
                return new BasePaginatedList<FavoriteDishResponse>(
                    response,
                    response.Count,
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

        public async Task CreateFavoriteDishAsync(FavoriteDishDTO favoriteDishDTO)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var existingFavoriteDish = await _unitOfWork.Repository<FavoriteDish>().FindAsync(
                x => x.SmartDietUserId == favoriteDishDTO.SmartDietUserId && x.DishId == favoriteDishDTO.DishId);

                if (existingFavoriteDish != null)
                    throw new ErrorException(
                        StatusCodes.Status400BadRequest,
                        ErrorCode.BADREQUEST,
                        "Favorite dish already exists!");

                var favoriteDish = _mapper.Map<FavoriteDish>(favoriteDishDTO);
                favoriteDish.CreatedTime = DateTime.UtcNow;
                favoriteDish.CreatedBy = userId;

                await _unitOfWork.Repository<FavoriteDish>().AddAsync(favoriteDish);
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

        public async Task UpdateFavoriteDishAsync(string favoriteDishId, FavoriteDishDTO favoriteDishDTO)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var existingFavoriteDish = await _unitOfWork.Repository<FavoriteDish>().GetByIdAsync(favoriteDishId)
                    ?? throw new ErrorException(
                        StatusCodes.Status404NotFound, 
                        ErrorCode.NOT_FOUND, 
                        "Favorite Dish not found!");

                _mapper.Map(favoriteDishDTO, existingFavoriteDish);
                existingFavoriteDish.LastUpdatedTime = DateTime.UtcNow;
                existingFavoriteDish.LastUpdatedBy = userId;

                await _unitOfWork.Repository<FavoriteDish>().UpdateAsync(existingFavoriteDish);
                await _unitOfWork.SaveChangeAsync();
            }
            catch(ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task DeleteFavoriteDishAsync(string favoriteDishId)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var favoriteDish = await _unitOfWork.Repository<FavoriteDish>().GetByIdAsync(favoriteDishId)
                    ?? throw new ErrorException(
                        StatusCodes.Status404NotFound, 
                        ErrorCode.NOT_FOUND, 
                        "Favorite Dish not found!");

                favoriteDish.DeletedTime = DateTime.UtcNow;
                favoriteDish.LastUpdatedBy = userId;
                _unitOfWork.Repository<FavoriteDish>().DeleteAsync(favoriteDish);
                await _unitOfWork.SaveChangeAsync();
            }
            catch(ErrorException)
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
