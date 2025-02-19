using DTOs.UserAllergyDTOs;
using DTOs.UserPreferenceDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IUserAllergyService
    {
        Task<IEnumerable<UserAllergyResponse>> GetUserAllergies();

        Task UpdateUserAllergies(List<string> foodIdsToAdd, List<string> foodIdsToRemove);
    }
}
