using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.UserFeedbackDTOs
{
    public class UserFeedbackDTO
    {

        public int StarRating { get; set; }

        public string Comment { get; set; } = string.Empty;
    }
}
