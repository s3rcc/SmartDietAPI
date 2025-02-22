using DTOs.UserProfileDTos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IUserProfileService
    {
        Task CreateUserProfileAsync(UserProfileDTO userProfileDTO);
        Task<UserProfileResponse> GetUserProfileAsync();
        Task UpdateUserProfileAsync(UserProfileDTO userProfileDTO);
    }
}
