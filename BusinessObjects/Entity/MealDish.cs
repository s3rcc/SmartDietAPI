using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Base;

namespace BusinessObjects.Entity
{
    public class MealDish : BaseEntity
    {
        public string MealId { get; set; }
        public Meal Meal { get; set; }
        public string DishId { get; set; }
        public Dish Dish { get; set; }
        public int? ServingSize;
    }
}
