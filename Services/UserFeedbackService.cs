using AutoMapper;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.FavoriteDishDTOs;
using DTOs.UserFeedbackDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class UserFeedbackService : IUserFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public UserFeedbackService(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task CreateUserFeedbackAsync(UserFeedbackDTO userFeedbackDTO)
        {

            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                var userFeedback = _mapper.Map<UserFeedback>(userFeedbackDTO);

                userFeedback.SmartDietUserId = userId;
                userFeedback.CreatedBy = userId;

                await _unitOfWork.Repository<UserFeedback>().AddAsync(userFeedback);
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

        public async Task DeleteUserFeedbackAsync(string id)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                var userFeedback = await _unitOfWork.Repository<UserFeedback>().GetByIdAsync(id)
                    ?? throw new ErrorException(
                        StatusCodes.Status404NotFound,
                        ErrorCode.NOT_FOUND,
                        "User Feedback not found!");

                _unitOfWork.Repository<UserFeedback>().DeleteAsync(userFeedback);
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

        public async Task<IEnumerable<UserFeedbackResponse>> GetUserFeedbackBySmartDietUserIdAsync(string id)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                var userFeedbacks = await _unitOfWork.Repository<UserFeedback>().FindAsync(
                    predicate: feedback => feedback.SmartDietUserId == id,
                    orderBy: query => query.OrderByDescending(f => f.FeedbackDate)
                    ) 
                    ?? throw new ErrorException(
                        StatusCodes.Status404NotFound, 
                        ErrorCode.NOT_FOUND, 
                        "User Feedback not found!");

                return _mapper.Map<IEnumerable<UserFeedbackResponse>>(userFeedbacks);
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

        public async Task<IEnumerable<UserFeedbackResponse>> GetAllUserFeedbackAsync()
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                var userFeedbacks = await _unitOfWork.Repository<UserFeedback>().GetAllAsync(
                    orderBy:  query => query.OrderByDescending(f => f.FeedbackDate)
                    )
                    ?? throw new ErrorException(
                        StatusCodes.Status404NotFound,
                        ErrorCode.NOT_FOUND,
                        "User Feedback not found!");

                return _mapper.Map<IEnumerable<UserFeedbackResponse>>(userFeedbacks);
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
