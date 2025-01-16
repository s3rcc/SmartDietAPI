using BusinessObjects.Entity;
using DTOs.UserFeedbackDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IUserFeedbackService
    {
        Task CreateUserFeedbackAsync(UserFeedbackDTO userFeedbackDTO);
        Task DeleteUserFeedbackAsync(string id);
        Task<IEnumerable<UserFeedbackResponse>> GetUserFeedbackBySmartDietUserIdAsync(string id);
        Task<IEnumerable<UserFeedbackResponse>> GetAllUserFeedbackAsync();
    }
}
