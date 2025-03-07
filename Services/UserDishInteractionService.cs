using AutoMapper;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.DishDTOs;
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
    public class UserDishInteractionService : IUserDishInteractionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public UserDishInteractionService(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task CreateUserDishInteractionAsync(UserDishInteractionDTO dto)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                var existing = await _unitOfWork.Repository<UserDishInteraction>().FindAsync(
                    x => x.SmartDietUserId == userId && x.DishId == dto.DishId && x.InteractionType == dto.InteractionType);

                if (existing.Any())
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Interaction already exists!");

                var interaction = _mapper.Map<UserDishInteraction>(dto);
                interaction.SmartDietUserId = userId;
                interaction.CreatedBy = userId;
                interaction.CreatedTime = DateTime.UtcNow;
                interaction.InteractionDate = DateTime.UtcNow;

                await _unitOfWork.Repository<UserDishInteraction>().AddAsync(interaction);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (ErrorException) { throw; }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task DeleteUserDishInteractionAsync(string id)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var interaction = await _unitOfWork.Repository<UserDishInteraction>().GetByIdAsync(id)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Interaction not found!");

                if (interaction.CreatedBy != userId)
                    throw new ErrorException(StatusCodes.Status403Forbidden, ErrorCode.FORBIDDEN, "Unauthorized deletion!");

                _unitOfWork.Repository<UserDishInteraction>().DeleteAsync(interaction);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (ErrorException) { throw; }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task<IEnumerable<UserDishInteractionResponse>> GetAllUserDishInteractionsAsync()
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var interactions = await _unitOfWork.Repository<UserDishInteraction>().FindAsync(
                    x => x.CreatedBy == userId, includes: x => x.Dish);

                return _mapper.Map<IEnumerable<UserDishInteractionResponse>>(interactions);
            }
            catch (ErrorException) { throw; }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task<UserDishInteractionResponse> GetUserDishInteractionByIdAsync(string id)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var interaction = await _unitOfWork.Repository<UserDishInteraction>().GetByIdAsync(id,
                    includes: x => x.Dish)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Interaction not found!");

                if (interaction.CreatedBy != userId)
                    throw new ErrorException(StatusCodes.Status403Forbidden, ErrorCode.FORBIDDEN, "Unauthorized access!");

                return _mapper.Map<UserDishInteractionResponse>(interaction);
            }
            catch (ErrorException) { throw; }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task<UserDishInteractionResponse> GetUserDishInteractionByDishIdAsync(string id)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var interaction = await _unitOfWork.Repository<UserDishInteraction>().FirstOrDefaultAsync(x => x.DishId == id,
                    includes: x => x.Dish)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Interaction not found!");

                if (interaction.CreatedBy != userId)
                    throw new ErrorException(StatusCodes.Status403Forbidden, ErrorCode.FORBIDDEN, "Unauthorized access!");

                return _mapper.Map<UserDishInteractionResponse>(interaction);
            }
            catch (ErrorException) { throw; }
            catch (Exception ex)
            {
                throw new ErrorException(StatusCodes.Status500InternalServerError, ErrorCode.INTERNAL_SERVER_ERROR, ex.Message);
            }
        }

        public async Task UpdateUserDishInteractionAsync(string id, UserDishInteractionDTO dto)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var interaction = await _unitOfWork.Repository<UserDishInteraction>().GetByIdAsync(id)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Interaction not found!");

                if (interaction.CreatedBy != userId)
                    throw new ErrorException(StatusCodes.Status403Forbidden, ErrorCode.FORBIDDEN, "Unauthorized update!");

                var existing = await _unitOfWork.Repository<UserDishInteraction>().FindAsync(
                    x => x.SmartDietUserId == userId && x.DishId == dto.DishId && x.InteractionType == dto.InteractionType && x.Id != id);

                if (existing.Any())
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Duplicate interaction detected!");

                _mapper.Map(dto, interaction);
                interaction.InteractionDate = DateTime.UtcNow;
                interaction.LastUpdatedBy = userId;
                interaction.LastUpdatedTime = DateTime.UtcNow;

                await _unitOfWork.Repository<UserDishInteraction>().UpdateAsync(interaction);
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
