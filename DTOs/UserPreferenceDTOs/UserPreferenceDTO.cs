using BusinessObjects.FixedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.UserPreferenceDTOs
{
    public class UserPreferenceDTO
    {
        // Dietary Preferences
        public DietType PrimaryDietType { get; set; }
        public List<RegionType> PrimaryRegionTypes { get; set; } = new List<RegionType>();
        public int DailyMealCount { get; set; }
        public int DishesPerMealCount { get; set; }

        // Cooking Preferences
        public int MaxCookingTime { get; set; }
        public DifficultyLevel MaxRecipeDifficulty { get; set; }
        public bool NotifyLowInventory { get; set; }
        public decimal LowInventoryThreshold { get; set; }
    }
}
