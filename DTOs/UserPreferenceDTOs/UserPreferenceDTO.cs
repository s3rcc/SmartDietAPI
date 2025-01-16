using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.UserPreferenceDTOs
{
    public class UserPreferenceDTO
    {
        public string SmartDietUserId { get; set; }

        // Dietary Preferences
        public string PrimaryDietType { get; set; }
        public string PrimaryRegionType { get; set; }
        public int DailyMealCount { get; set; }
        public int DishesPerMealCount { get; set; }

        // Cooking Preferences
        public int MaxCookingTime { get; set; }
        public string MaxRecipeDifficulty { get; set; }
        public bool NotifyLowInventory { get; set; }
        public decimal LowInventoryThreshold { get; set; }
    }
}
