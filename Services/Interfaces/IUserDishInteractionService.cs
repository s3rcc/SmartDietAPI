using DTOs.DishDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IUserDishInteractionService
    {
        Task CreateUserDishInteractionAsync(UserDishInteractionDTO dto);
        Task DeleteUserDishInteractionAsync(string id);
        Task<IEnumerable<UserDishInteractionResponse>> GetAllUserDishInteractionsAsync();
        Task<UserDishInteractionResponse> GetUserDishInteractionByDishIdAsync(string id);
        Task<UserDishInteractionResponse> GetUserDishInteractionByIdAsync(string id);
        Task UpdateUserDishInteractionAsync(string id, UserDishInteractionDTO dto);
    }
}
