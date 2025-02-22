using BusinessObjects.Base;
using DTOs.UserPreferenceDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{

    public interface IUserPreferenceService
    {
        Task<UserPreferenceResponse> GetUserPreferenceByIdAsync();

        Task CreateUserPreferenceAsync(UserPreferenceDTO userPreferenceDto);

        Task UpdateUserPreferenceAsync(UserPreferenceDTO userPreferenceDto);

        #region Cumtumlum
        //Task<IEnumerable<UserPreferenceResponse>> GetAllUserPreferencesAsync();
        //Task<BasePaginatedList<UserPreferenceResponse>> GetAllUserPreferencesAsync(int pageIndex, int pageSize, string? searchTerm);
        #endregion

    }
}
