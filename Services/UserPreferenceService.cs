using AutoMapper;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.UserPreferenceDTOs;
using Microsoft.AspNetCore.Http;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class UserPreferenceService : IUserPreferenceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserPreferenceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserPreferenceResponse> GetUserPreferenceByIdAsync(string id)
        {
            try
            {
                var userPreference = await _unitOfWork.Repository<UserPreference>().GetByIdAsync(id)
                                    ?? throw new ErrorException(
                                        StatusCodes.Status404NotFound,
                                        ErrorCode.NOT_FOUND,
                                        "User preference not found!");
                return _mapper.Map<UserPreferenceResponse>(userPreference);
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
                //var existingPreference = await _unitOfWork.Repository<UserPreference>().FindAsync(
                //    x => x.SmartDietUserId == userPreferenceDto.SmartDietUserId);

                //if (existingPreference != null)
                //    throw new ErrorException(
                //        StatusCodes.Status400BadRequest,
                //        ErrorCode.BADREQUEST,
                //        "User preference already exists!");

                var userPreference = _mapper.Map<UserPreference>(userPreferenceDto);
                userPreference.CreatedTime = DateTime.UtcNow;
                userPreference.CreatedBy = "system";

                await _unitOfWork.Repository<UserPreference>().AddAsync(userPreference);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task UpdateUserPreferenceAsync(string userPreferenceId, UserPreferenceDTO userPreferenceDto)
        {
            try
            {
                var existingPreference = await _unitOfWork.Repository<UserPreference>().GetByIdAsync(userPreferenceId)
                                      ?? throw new ErrorException(
                                          StatusCodes.Status404NotFound,
                                          ErrorCode.NOT_FOUND,
                                          "User preference not found!");

                _mapper.Map(userPreferenceDto, existingPreference);
                existingPreference.LastUpdatedTime = DateTime.UtcNow;

                await _unitOfWork.Repository<UserPreference>().UpdateAsync(existingPreference);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }
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