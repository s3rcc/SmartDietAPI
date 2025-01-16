using BusinessObjects.Base;
using BusinessObjects.FixedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entity
{
    public class Dish : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string? Video { get; set; }
        public string Instruction { get; set; }
        public int PrepTimeMinutes { get; set; }
        public int CookingTimeMinutes { get; set; }
        public DifficultyLevel Difficulty { get; set; }
        public RegionType RegionType { get; set; }
        public DietType? DietType { get; set; }

        public ICollection<FavoriteDish>? FavoriteDishes { get; set; }
        public ICollection<DishIngredient> DishIngredients { get; set; }
    }
}
