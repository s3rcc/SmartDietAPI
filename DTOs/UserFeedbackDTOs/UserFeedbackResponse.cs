using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.UserFeedbackDTOs
{
    public class UserFeedbackResponse
    {
        public string Id { get; set; }

        public string SmartDietUserId { get; set; }

        public int StarRating { get; set; }

        public string Comment { get; set; } = string.Empty;
    }
}
