using BusinessObjects.Base;
using DTOs.MealDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IUserMealInteractionService
    {
        Task CreateUserMealInteractionAsync(UserMealInteractionDTO dto);
        Task<UserMealInteractionResponse> CreateMealInteractionAsync(UserMealInteractionDTO dto);
        Task DeleteUserMealInteractionAsync(string id);
        Task<IEnumerable<UserMealInteractionResponse>> GetAllUserMealInteractionsAsync();
        Task<UserMealInteractionResponse> GetUserMealInteractionByMealIdAsync(string id);
        Task<UserMealInteractionResponse> GetUserMealInteractionByIdAsync(string id);
        Task UpdateUserMealInteractionAsync(string id, UserMealInteractionDTO dto);
    }
}
