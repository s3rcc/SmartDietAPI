using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.UserFeedbackDTOs
{
    public class UserFeedbackResponse
    {
        public string Id { get; set; } = string.Empty;

        public string SmartDietUserId { get; set; } = string.Empty;

        //public string SmartDietUserName { get; set; } = string.Empty;

        public int StarRating { get; set; }

        public string Comment { get; set; } = string.Empty;
    }
}
