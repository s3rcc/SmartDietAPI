using AutoMapper;
using BusinessObjects.Entity;
using BusinessObjects.Exceptions;
using DTOs.FridgeDTOs;
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
    public class FridgeService : IFridgeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public FridgeService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
        }
        public async Task AddItemsToFridge(string fridgeId, List<FridgeItemDTO> fridgeItemDTOs)
        {

            throw new NotImplementedException();

        }

        public async Task CreateFridgeAsync(FridgeDTO fridgeDTO)
        {
            var fridgeLimit = int.Parse(_configuration["FridgeSettings:FridgeLimit"]);
            // Check existing fridge
            var existingFridge = await _unitOfWork.Repository<Fridge>().GetAllAsync();
            // limit the number of fridge
            if (existingFridge.Count() > fridgeLimit)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, $"Limit number of fridge {fridgeLimit}");
            }
            var fridge = _mapper.Map<Fridge>(fridgeDTO);
            // Set created time
            fridge.CreatedTime = DateTime.UtcNow;
            fridge.CreatedBy = "System";
            // Set user Id for fridge
            fridge.SmartDietUserId = "System";
            await _unitOfWork.Repository<Fridge>().AddAsync(fridge);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task DeleteFridgeAsync(string id)
        {
            // Check existing fridge
            var existingFridge = await _unitOfWork.Repository<Fridge>().GetByIdAsync(id)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Fridge does not exist");
            // Check fridge user match
            //existingFridge.CreatedBy = user;
            existingFridge.DeletedTime = DateTime.UtcNow;
            existingFridge.DeletedBy = "System";
            await _unitOfWork.Repository<Fridge>().UpdateAsync(existingFridge);
            await _unitOfWork.SaveChangeAsync();

        }

        public async Task<IEnumerable<FridgeItemResponse>> GetAllItemsInFridge(string id)
        {
            // Check if fridge exists
            var fridge = await _unitOfWork.Repository<Fridge>().GetByIdAsync(id)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Fridge does not exist");
            // Get list of item in fridge
            var fridgeItems = await _unitOfWork.Repository<FridgeItem>().FindAsync(x => x.FridgeId == id,
                includes: x => x.Food);
            // Return list
            return _mapper.Map<IEnumerable<FridgeItemResponse>>(fridgeItems);

        }

        public async Task<IEnumerable<FridgeRespose>> GetAllUserFrige()
        {
            // Get user id
            var userId = "abc";
            // Get all fridge of the user
            var fridges = await _unitOfWork.Repository<Fridge>().FindAsync(x => x.SmartDietUserId == userId);
            return _mapper.Map<IEnumerable<FridgeRespose>>(fridges);
        }

        public async Task<FridgeRespose> GetFridgeByIdAsync(string id)
        {
            // Check if fridge exists
            var fridge = await _unitOfWork.Repository<Fridge>().GetByIdAsync(id)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Fridge does not exist");
            return _mapper.Map<FridgeRespose>(fridge);
        }

        public async Task<FridgeItemResponse> GetItemById(string id)
        {
            // Check if fridge exists
            var fridgeItem = await _unitOfWork.Repository<FridgeItem>().GetByIdAsync(id, includes: x => x.Food)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Item does not exist");
            return _mapper.Map<FridgeItemResponse>(fridgeItem);
        }

        public Task RemoveItemsFromFridge(string fridgeId, string itemId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateFridgeAsync(string Id, FridgeDTO fridgeDTO)
        {
            throw new NotImplementedException();
        }

        public Task UpdateItemInFridge(string itemId, FridgeItemDTO fridgeItemDTO)
        {
            throw new NotImplementedException();
        }
    }
}
