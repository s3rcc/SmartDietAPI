using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Base;

namespace BusinessObjects.Entity
{
    public class FavoriteMeal : BaseEntity
    {
        public string SmartDietUserId { get; set; }
        public SmartDietUser SmartDietUser { get; set; }
        public string MealId { get; set; }
        public Meal Meal { get; set; }
    }
}
