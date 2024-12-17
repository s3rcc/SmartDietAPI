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

            // Check if fridge exists
            var fridge = await _unitOfWork.Repository<Fridge>().GetByIdAsync(fridgeId)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Fridge does not exist");

            // Map FridgeItemDTOs to FridgeItem entities
            var fridgeItems = _mapper.Map<List<FridgeItem>>(fridgeItemDTOs);

            foreach (var item in fridgeItems)
            {
                // Validate item (e.g., expiration date cannot be earlier than purchase date)
                if (item.ExpirationDate < item.PurchaseDate)
                {
                    throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Invalid expiration date");
                }

                // Set required properties
                item.CreatedTime = DateTime.UtcNow;
                item.CreatedBy = "System"; // Replace with the actual user ID if applicable
                item.FridgeId = fridgeId;
            }

            // Add items to the database
            await _unitOfWork.Repository<FridgeItem>().AddRangeAsync(fridgeItems);
            await _unitOfWork.SaveChangeAsync();

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

        public async Task RemoveItemsFromFridge(string fridgeId, string itemId)
        {
            // Check if fridge exists
            var fridge = await _unitOfWork.Repository<Fridge>().GetByIdAsync(fridgeId)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Fridge does not exist");

            // Check if item exists
            var fridgeItem = await _unitOfWork.Repository<FridgeItem>().FirstOrDefaultAsync(x => x.FridgeId == fridgeId && x.Id == itemId);
            if (fridgeItem == null)
            {
                throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Item does not exist in the fridge");
            }

            // Remove the item
            _unitOfWork.Repository<FridgeItem>().DeleteAsync(fridgeItem);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task UpdateFridgeAsync(string id, FridgeDTO fridgeDTO)
        {
            // Check if fridge exists
            var existingFridge = await _unitOfWork.Repository<Fridge>().GetByIdAsync(id)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Fridge does not exist");

            // Update fridge properties
            existingFridge.FridgeModel = fridgeDTO.FridgeModel;
            existingFridge.FridgeLocation = fridgeDTO.FridgeLocation;
            existingFridge.LastUpdatedTime = DateTime.UtcNow;
            existingFridge.LastUpdatedBy = "System"; // Replace with the actual user if available

            // Save changes
            await _unitOfWork.Repository<Fridge>().UpdateAsync(existingFridge);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task UpdateItemInFridge(string itemId, FridgeItemDTO fridgeItemDTO)
        {
            // Find the item by ID
            var existingItem = await _unitOfWork.Repository<FridgeItem>().GetByIdAsync(itemId)
                ?? throw new ErrorException(StatusCodes.Status404NotFound, ErrorCode.NOT_FOUND, "Item does not exist");

            // Validate expiration date
            if (fridgeItemDTO.ExpirationDate < fridgeItemDTO.PurchaseDate)
            {
                throw new ErrorException(StatusCodes.Status400BadRequest, ErrorCode.BADREQUEST, "Invalid expiration date");
            }

            // Update fields
            existingItem.Quantity = fridgeItemDTO.Quantity;
            existingItem.PurchaseDate = fridgeItemDTO.PurchaseDate;
            existingItem.ExpirationDate = fridgeItemDTO.ExpirationDate;
            existingItem.StorageLocation = fridgeItemDTO.StorageLocation;
            existingItem.Notes = fridgeItemDTO.Notes;
            existingItem.LastUpdatedTime = DateTime.UtcNow;
            existingItem.LastUpdatedBy = "System"; // Replace with the actual user if available

            // Save changes
            await _unitOfWork.Repository<FridgeItem>().UpdateAsync(existingItem);
            await _unitOfWork.SaveChangeAsync();
        }
    }
}
