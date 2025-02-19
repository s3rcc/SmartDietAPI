using AutoMapper;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.UserAllergyDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class UserAllergyService : IUserAllergyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public UserAllergyService(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task UpdateUserAllergies(List<string> foodIdsToAdd, List<string> foodIdsToRemove)
        {
            try
            {
                // Lấy userId từ token
                var userId = _tokenService.GetUserIdFromToken();

                // Xử lý thêm dị ứng mới
                if (foodIdsToAdd.Any())
                {
                    await AddUserAllergies(foodIdsToAdd.Select(foodId => new UserAllergyDTO { FoodId = foodId }).ToList());
                }

                // Xử lý xóa dị ứng
                if (foodIdsToRemove.Any())
                {
                    await RemoveUserAllergies(foodIdsToRemove);
                }
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "Something went wrong");
            }
        }

        public async Task AddUserAllergies(List<UserAllergyDTO> userAllergyDTOs)
        {
            try
            {
                // Lấy userId từ token
                var userId = _tokenService.GetUserIdFromToken();

                // Lấy danh sách các thực phẩm dị ứng hiện tại của người dùng
                var existingAllergies = await _unitOfWork.Repository<UserAllergy>()
                    .FindAsync(x => x.SmartDietUserId == userId);

                // Lọc ra các thực phẩm dị ứng mới chưa tồn tại trong danh sách hiện tại
                var newAllergies = userAllergyDTOs
                    .Where(dto => !existingAllergies.Any(x => x.FoodId == dto.FoodId))
                    .ToList();

                if (!newAllergies.Any())
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "All allergies already exist for the user");
                }

                // Map DTOs sang entities
                var userAllergiesToAdd = _mapper.Map<List<UserAllergy>>(newAllergies);

                foreach (var allergy in userAllergiesToAdd)
                {
                    // Thiết lập các thuộc tính cần thiết
                    allergy.SmartDietUserId = userId;
                    allergy.CreatedTime = DateTime.UtcNow;
                    allergy.CreatedBy = userId;
                }

                // Thêm các dị ứng mới vào cơ sở dữ liệu
                await _unitOfWork.Repository<UserAllergy>().AddRangeAsync(userAllergiesToAdd);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "Something went wrong");
            }
        }

        public async Task RemoveUserAllergies(List<string> foodIds)
        {
            try
            {
                // Lấy userId từ token
                var userId = _tokenService.GetUserIdFromToken();

                // Lấy danh sách các dị ứng cần xóa
                var allergiesToDelete = await _unitOfWork.Repository<UserAllergy>()
                    .FindAsync(x => x.SmartDietUserId == userId && foodIds.Contains(x.FoodId));

                if (!allergiesToDelete.Any())
                {
                    throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "No allergies found to delete");
                }

                // Xóa các dị ứng
                _unitOfWork.Repository<UserAllergy>().DeleteRangeAsync(allergiesToDelete);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "Something went wrong");
            }
        }
        public async Task<IEnumerable<UserAllergyResponse>> GetUserAllergies()
        {
            try
            {

                // Lấy userId từ token
                var userId = _tokenService.GetUserIdFromToken();

                // Lấy danh sách các dị ứng của người dùng
                var userAllergies = await _unitOfWork.Repository<UserAllergy>()
                    .FindAsync(x => x.SmartDietUserId == userId, includes: x => x.Food);

                // Map sang DTO và trả về
                return _mapper.Map<IEnumerable<UserAllergyResponse>>(userAllergies);
            }
            catch (ErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, "Some thing wennt wrong");
            }
        }
    }
}
