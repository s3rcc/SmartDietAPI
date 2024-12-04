using BusinessObjects.FixedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.DishDTOs
{
    public class DishIngredientResponse
    {
        public string FoodId { get; set; }
        public string FoodName { get; set; }
        public double Quantity { get; set; }
        public MeasurementUnit MeasurementUnit { get; set; }
    }
}
