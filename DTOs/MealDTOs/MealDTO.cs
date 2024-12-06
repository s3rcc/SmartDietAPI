using BusinessObjects.FixedData;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.MealDTOs
{
    public class MealDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DietType DietType { get; set; }
        public IFormFile? Image { get; set; }
        public List<string> DishIds { get; set; }
    }
}
