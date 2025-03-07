using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.FixedData;

namespace DTOs.UserPreferenceDTOs
{
    public class UserPreferenceResponse
    {
        public string Id { get; set; }

        // Dietary Preferences
        public string PrimaryDietType { get; set; }
        public List<RegionType> PrimaryRegionTypes { get; set; } = new List<RegionType>();
        public int DailyMealCount { get; set; }
        public int DishesPerMealCount { get; set; }

        // Cooking Preferences
        public int MaxCookingTime { get; set; }
        public string MaxRecipeDifficulty { get; set; }
        public bool NotifyLowInventory { get; set; }
        public decimal LowInventoryThreshold { get; set; }
    }
}
