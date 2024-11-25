using BusinessObjects.Base;
using BusinessObjects.FixedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entity
{
    public class Meal : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DietType DietType { get; set; }
        public string? Image { get; set; }
        public ICollection<FavoriteMeal>? FavoriteMeals { get; set; }
        public ICollection<MealDish> MealDishes { get; set; }
    }
}
