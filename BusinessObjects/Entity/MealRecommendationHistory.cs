using BusinessObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entity
{
    public class MealRecommendationHistory : BaseEntity
    {
        public string SmartDietUserId { get; set; }
        public SmartDietUser SmartDietUser { get; set; }
        public string MealId { get; set; }
        public Meal Meal { get; set; }
        public DateTime RecommendationDate { get; set; }
    }
}
