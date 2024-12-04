using BusinessObjects.Entity;
using BusinessObjects.FixedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.DishDTOs
{
    public class DishResponse
    {
        public string Id {  get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? Video { get; set; }
        public string Instruction { get; set; }
        public int PrepTimeMinutes { get; set; }
        public int CookingTimeMinutes { get; set; }
        public RegionType RegionType { get; set; }
        public DietType? DietType { get; set; }
        public ICollection<DishIngredientResponse> DishIngredients { get; set; } = new List<DishIngredientResponse>();
        public DateTime CreatedTime { get; set; }

        public DateTime? LastUpdatedTime { get; set; }

        public DateTime? DeletedTime { get; set; }
    }
}
