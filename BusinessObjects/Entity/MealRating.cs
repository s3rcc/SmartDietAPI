using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Base;

namespace BusinessObjects.Entity
{
    public class MealRating : BaseEntity
    {
        public string SmartDietUserId { get; set; }
        public SmartDietUser SmartDietUser { get; set; }
        public string MealId { get; set; }
        public Meal Meal { get; set; }
        [Required]
        public int Rating { get; set; }
        public string FeedBack {  get; set; }
    }
}
