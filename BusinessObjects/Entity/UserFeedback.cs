using BusinessObjects.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entity
{
    public class UserFeedback : BaseEntity
    {
        public string SmartDietUserId { get; set; } = string.Empty; 

        [Range(1, 5)]
        public int StarRating { get; set; }  

        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime FeedbackDate { get; set; } = DateTime.UtcNow;  

        public SmartDietUser? SmartDietUser { get; set; }
    }

}
