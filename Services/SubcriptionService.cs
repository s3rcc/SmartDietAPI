using AutoMapper;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.DishDTOs;
using DTOs.MealDTOs;
using DTOs.SubcriptionDTOs;
using DTOs.UserAllergyDTOs;
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
    public class SubcriptionService : ISubcriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public SubcriptionService(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        public async Task CreateSubcriptionAsync(SubcriptionRequest request)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();
                var existingSubcription = await _unitOfWork.Repository<Subcription>().FirstOrDefaultAsync(x => x.Name == request.Name);
                if (existingSubcription != null)
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Subcription name already exists");
                }
                var subcription = _mapper.Map<Subcription>(request);
                subcription.CreatedTime = DateTime.UtcNow;
                subcription.CreatedBy = userId;

                await _unitOfWork.Repository<Subcription>().AddAsync(subcription);
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

        public async Task DeleteSubcriptionAsync(string Id)
        {
            try
            {
                var subcription = await _unitOfWork.Repository<Subcription>().GetByIdAsync(Id)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Subcription does not exist!");

                // Check if meal is favorited

                subcription.DeletedTime = DateTime.UtcNow;
                subcription.DeletedBy = _tokenService.GetUserIdFromToken();

                await _unitOfWork.Repository<Subcription>().UpdateAsync(subcription);
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

        public async Task<IEnumerable<SubcriptionResponse>> GetAllSubcriptionAsync()
        {
            try
            {
                var subcriptions = await _unitOfWork.Repository<Subcription>().GetAllAsync();

                return _mapper.Map<IEnumerable<SubcriptionResponse>>(subcriptions);
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

        public async Task<SubcriptionResponse> GetSubcriptionByIdAsync(string Id)
        {
            try
            {
                var subcription = await _unitOfWork.Repository<Subcription>().GetByIdAsync(Id)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Subcription does not exist!");

                return _mapper.Map<SubcriptionResponse>(subcription);
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

        public async Task UpdateSubcriptionAsync(string Id, SubcriptionRequest request)
        {
            try
            {
                var userId = _tokenService.GetUserIdFromToken();

                var existingSubcription = await _unitOfWork.Repository<Subcription>().GetByIdAsync(Id)
                    ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Subcription does not exist!");

                var existingName = await _unitOfWork.Repository<Subcription>().FirstOrDefaultAsync(x => x.Name == request.Name && x.Id != Id);
                if (existingName != null)
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Subcription name already exists");
                }

                _mapper.Map(request, existingSubcription);

                existingSubcription.LastUpdatedTime = DateTime.UtcNow;
                existingSubcription.LastUpdatedBy = userId;

                await _unitOfWork.Repository<Subcription>().UpdateAsync(existingSubcription);
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
