using AutoMapper;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.UserProfileDTos;
using Microsoft.AspNetCore.Http;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ITokenService _tokenService;

        public UserProfileService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICloudinaryService cloudinaryService,
            ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
            _tokenService = tokenService;
        }

        public async Task CreateUserProfileAsync(UserProfileDTO userProfileDTO)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                // Check if the user profile already exists
                var existingProfile = await _unitOfWork.Repository<UserProfile>().FirstOrDefaultAsync(
                    x => x.SmartDietUserId == userId);

                if (existingProfile != null)
                {
                    throw new ErrorException(
                        StatusCodes.Status400BadRequest,
                        ErrorCode.BADREQUEST,
                        "User profile already exists!");
                }

                // Map DTO to entity
                var userProfile = _mapper.Map<UserProfile>(userProfileDTO);
                userProfile.SmartDietUserId = userId;

                // Process profile picture
                if (userProfileDTO.ProfilePicture != null)
                {
                    userProfile.ProfilePicture = await _cloudinaryService.UploadImageAsync(userProfileDTO.ProfilePicture);
                }

                // Set created time and user
                userProfile.CreatedTime = DateTime.UtcNow;
                userProfile.CreatedBy = userId;

                // Save to database
                await _unitOfWork.Repository<UserProfile>().AddAsync(userProfile);
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

        public async Task<UserProfileResponse> GetUserProfileAsync()
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var userProfile = await _unitOfWork.Repository<UserProfile>().FirstOrDefaultAsync(x => x.SmartDietUserId == userId)
                                  ?? throw new ErrorException(
                                      StatusCodes.Status404NotFound,
                                      ErrorCode.NOT_FOUND,
                                      "User profile not found!");

                return _mapper.Map<UserProfileResponse>(userProfile);
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

        public async Task UpdateUserProfileAsync(UserProfileDTO userProfileDTO)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                // Retrieve existing profile
                var existingProfile = await _unitOfWork.Repository<UserProfile>().FirstOrDefaultAsync(x => x.SmartDietUserId == userId)
                                      ?? throw new ErrorException(
                                          StatusCodes.Status404NotFound,
                                          ErrorCode.NOT_FOUND,
                                          "User profile not found!");

                // Ensure the user is updating their own profile
                if (existingProfile.SmartDietUserId != userId)
                {
                    throw new ErrorException(
                        StatusCodes.Status403Forbidden,
                        ErrorCode.FORBIDDEN,
                        "You are not authorized to update this profile!");
                }

                // Retrieve old image URL
                var oldImageUrl = existingProfile.ProfilePicture;

                // Map DTO to entity
                _mapper.Map(userProfileDTO, existingProfile);

                // Process profile picture
                if (userProfileDTO.ProfilePicture != null)
                {
                    // Upload new image
                    existingProfile.ProfilePicture = await _cloudinaryService.UploadImageAsync(userProfileDTO.ProfilePicture);

                    // Delete old image if it exists
                    if (!string.IsNullOrEmpty(oldImageUrl))
                    {
                        var publicId = oldImageUrl.Split('/').Last().Split('.')[0];
                        await _cloudinaryService.DeleteImageAsync(publicId);
                    }
                }
                else
                {
                    // Keep old image
                    existingProfile.ProfilePicture = oldImageUrl;
                }

                // Set last updated time and user
                existingProfile.LastUpdatedTime = DateTime.UtcNow;
                existingProfile.LastUpdatedBy = userId;

                // Save changes to the database
                await _unitOfWork.Repository<UserProfile>().UpdateAsync(existingProfile);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (ErrorException)
            {
                throw;
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
    }
}