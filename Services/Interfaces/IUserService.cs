using BusinessObjects.Base;
using DTOs.UserProfileDTos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IUserService
    {
        Task UpdateUserProfiles(UpdateUserProfileRequest input);
        Task AddUserWithRoleAsync(RegisterUserWithRoleRequest request);
        Task<BasePaginatedList<UserProfileResponse>> GetAllUserProfileAsync(int pageIndex, int pageSize, string? searchTerm);
        Task<IEnumerable<UserProfileResponse>> GetAllUserProfile();
        Task<UserProfileResponse> GetUserProfile();
        Task DeleteUser(string userId);

    }
}
