using BusinessObjects.FixedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.MealDTOs
{
    public class MealResponse
    {
        public string Name {  get; set; }
        public string Description { get; set; }
        public DietType DietType { get; set; }
        public string? Image { get; set; }
        public ICollection<MealDishResponse> MealDishes { get; set; }

    }
}
