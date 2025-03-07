using AutoMapper;
using BusinessObjects.Base;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.MealDTOs;
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
    public class UserMealInteractionService : IUserMealInteractionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public UserMealInteractionService(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task CreateUserMealInteractionAsync(UserMealInteractionDTO dto)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                var existing = await _unitOfWork.Repository<UserMealInteraction>().FindAsync(
                    x => x.SmartDietUserId == userId && x.MealId == dto.MealId && x.InteractionType == dto.InteractionType);

                if (existing.Any())
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Interaction already exists!");

                var interaction = _mapper.Map<UserMealInteraction>(dto);
                interaction.SmartDietUserId = userId;
                interaction.CreatedBy = userId;
                interaction.CreatedTime = DateTime.UtcNow;
                interaction.LastInteractionTime = DateTime.UtcNow;

                await _unitOfWork.Repository<UserMealInteraction>().AddAsync(interaction);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (ErrorException) { throw; }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task DeleteUserMealInteractionAsync(string id)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var interaction = await _unitOfWork.Repository<UserMealInteraction>().GetByIdAsync(id)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Interaction not found!");

                if (interaction.CreatedBy != userId)
                    throw new ErrorException(StatusCodes.Status403Forbidden, ErrorCode.FORBIDDEN, "Unauthorized deletion!");

                _unitOfWork.Repository<UserMealInteraction>().DeleteAsync(interaction);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (ErrorException) { throw; }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task<IEnumerable<UserMealInteractionResponse>> GetAllUserMealInteractionsAsync()
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var interactions = await _unitOfWork.Repository<UserMealInteraction>().FindAsync(
                    x => x.CreatedBy == userId, includes: x => x.Meal);

                return _mapper.Map<IEnumerable<UserMealInteractionResponse>>(interactions);
            }
            catch (ErrorException) { throw; }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task<UserMealInteractionResponse> GetUserMealInteractionByIdAsync(string id)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var interaction = await _unitOfWork.Repository<UserMealInteraction>().GetByIdAsync(id,
                    includes: x => x.Meal)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Interaction not found!");

                if (interaction.CreatedBy != userId)
                    throw new ErrorException(StatusCodes.Status403Forbidden, ErrorCode.FORBIDDEN, "Unauthorized access!");

                return _mapper.Map<UserMealInteractionResponse>(interaction);
            }
            catch (ErrorException) { throw; }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task<UserMealInteractionResponse> GetUserMealInteractionByMealIdAsync(string id)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var interaction = await _unitOfWork.Repository<UserMealInteraction>().FirstOrDefaultAsync(x => x.MealId == id, 
                    includes: x => x.Meal)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Interaction not found!");

                if (interaction.CreatedBy != userId)
                    throw new ErrorException(StatusCodes.Status403Forbidden, ErrorCode.FORBIDDEN, "Unauthorized access!");

                return _mapper.Map<UserMealInteractionResponse>(interaction);
            }
            catch (ErrorException) { throw; }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task UpdateUserMealInteractionAsync(string id, UserMealInteractionDTO dto)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var interaction = await _unitOfWork.Repository<UserMealInteraction>().GetByIdAsync(id)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Interaction not found!");

                if (interaction.CreatedBy != userId)
                    throw new ErrorException(StatusCodes.Status403Forbidden, ErrorCode.FORBIDDEN, "Unauthorized update!");

                var existing = await _unitOfWork.Repository<UserMealInteraction>().FindAsync(
                    x => x.SmartDietUserId == userId && x.MealId == dto.MealId && x.InteractionType == dto.InteractionType && x.Id != id);

                if (existing.Any())
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Duplicate interaction detected!");

                _mapper.Map(dto, interaction);
                interaction.LastInteractionTime = DateTime.UtcNow;
                interaction.LastUpdatedBy = userId;
                interaction.LastUpdatedTime = DateTime.UtcNow;

                await _unitOfWork.Repository<UserMealInteraction>().UpdateAsync(interaction);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (ErrorException) { throw; }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }
    }
}
