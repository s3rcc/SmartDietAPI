using BusinessObjects.Base;
using BusinessObjects.FixedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entity
{
    public class DishIngredient : BaseEntity
    {
        public int DishId { get; set; }
        public int FoodId { get; set; }
        public double Quantity { get; set; }
        public MeasurementUnit MeasurementUnit { get; set; }
        public Dish Dish { get; set; }
        public Food Food { get; set; }
    }
}
