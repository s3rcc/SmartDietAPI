using AutoMapper;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using BusinessObjects.FixedData;
using DTOs.UserPreferenceDTOs;
using Microsoft.AspNetCore.Http;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class UserPreferenceService : IUserPreferenceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public UserPreferenceService(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        private RegionType CombineRegionTypes(List<RegionType> regionTypes)
        {
            RegionType combined = RegionType.None;
            foreach (var type in regionTypes)
            {
                combined |= type;
            }
            return combined;
        }

        private List<RegionType> SplitRegionTypes(RegionType combinedType)
        {
            return Enum.GetValues(typeof(RegionType))
                .Cast<RegionType>()
                .Where(r => r != RegionType.None && combinedType.HasFlag(r))
                .ToList();
        }

        public async Task<UserPreferenceResponse> GetUserPreferenceByIdAsync()
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                var userPreference = await _unitOfWork.Repository<UserPreference>().FirstOrDefaultAsync(x => x.SmartDietUserId == userId)
                                    ?? throw new ErrorException(
                                        StatusCodes.Status404NotFound,
                                        ErrorCode.NOT_FOUND,
                                        "User preference not found!");

                var response = _mapper.Map<UserPreferenceResponse>(userPreference);
                response.PrimaryRegionTypes = SplitRegionTypes(userPreference.PrimaryRegionType);
                return response;
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

        public async Task CreateUserPreferenceAsync(UserPreferenceDTO userPreferenceDto)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                var existingPreference = await _unitOfWork.Repository<UserPreference>().FindAsync(
                    x => x.SmartDietUserId == userId);

                if (existingPreference.Any())
                {
                    throw new ErrorException(
                        StatusCodes.Status400BadRequest,
                        ErrorCode.BADREQUEST,
                        "User preference already exists for this user!");
                }

                var userPreference = _mapper.Map<UserPreference>(userPreferenceDto);
                userPreference.SmartDietUserId = userId;
                userPreference.CreatedTime = DateTime.UtcNow;
                userPreference.CreatedBy = userId;
                userPreference.PrimaryRegionType = CombineRegionTypes(userPreferenceDto.PrimaryRegionTypes);

                await _unitOfWork.Repository<UserPreference>().AddAsync(userPreference);
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

        public async Task UpdateUserPreferenceAsync(UserPreferenceDTO userPreferenceDto)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                var existingPreference = await _unitOfWork.Repository<UserPreference>().FirstOrDefaultAsync(x => x.SmartDietUserId == userId)
                                      ?? throw new ErrorException(
                                          StatusCodes.Status404NotFound,
                                          ErrorCode.NOT_FOUND,
                                          "User preference not found!");

                if (existingPreference.CreatedBy != userId)
                {
                    throw new ErrorException(
                        StatusCodes.Status403Forbidden,
                        ErrorCode.FORBIDDEN,
                        "You are not authorized to update this user preference!");
                }

                _mapper.Map(userPreferenceDto, existingPreference);
                existingPreference.PrimaryRegionType = CombineRegionTypes(userPreferenceDto.PrimaryRegionTypes);
                existingPreference.LastUpdatedTime = DateTime.UtcNow;
                existingPreference.LastUpdatedBy = userId;

                await _unitOfWork.Repository<UserPreference>().UpdateAsync(existingPreference);
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

        //public async Task<IEnumerable<UserPreferenceResponse>> GetAllUserPreferencesAsync()
        //{
        //    try
        //    {
        //        var userPreferences = await _unitOfWork.Repository<UserPreference>().GetAllAsync();
        //        return _mapper.Map<IEnumerable<UserPreferenceResponse>>(userPreferences);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
        //    }
        //}

        //public async Task<BasePaginatedList<UserPreferenceResponse>> GetAllUserPreferencesAsync(int pageIndex, int pageSize, string? searchTerm)
        //{
        //    try
        //    {
        //        var userPreferences = await _unitOfWork.Repository<UserPreference>().GetAllWithPaginationAsync(
        //            pageIndex,
        //            pageSize,
        //            searchTerm: x => string.IsNullOrEmpty(searchTerm) || x.SmartDietUserId.Contains(searchTerm),
        //            orderBy: x => x.OrderBy(p => p.SmartDietUserId)
        //        );

        //        if (userPreferences == null || !userPreferences.Items.Any())
        //        {
        //            return new BasePaginatedList<UserPreferenceResponse>(
        //                new List<UserPreferenceResponse>(),
        //                0,
        //                pageIndex,
        //                pageSize);
        //        }

        //        var response = _mapper.Map<List<UserPreferenceResponse>>(userPreferences.Items);
        //        return new BasePaginatedList<UserPreferenceResponse>(
        //            response,
        //            response.Count,
        //            pageIndex,
        //            pageSize);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
        //    }
        //}
    }
}