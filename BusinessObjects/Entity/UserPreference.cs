using BusinessObjects.FixedData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Base;

namespace BusinessObjects.Entity
{
    public class UserPreference : BaseEntity
    {
        public string SmartDietUserId { get; set; }
        public SmartDietUser SmartDietUser { get; set; }
        // Dietary Preferences
        [Required]
        public DietType PrimaryDietType { get; set; } = DietType.Regular;

        [Required]
        public RegionType PrimaryRegionType { get; set; }
        [Required]
        public int DailyMealCount { get; set; } = 3;
        public int DishesPerMealCount { get; set; } = 3;

        // Cooking Preferences
        [Range(0, 180)]
        public int MaxCookingTime { get; set; } = 60;

        public DifficultyLevel MaxRecipeDifficulty { get; set; } = DifficultyLevel.Medium;

        public bool NotifyLowInventory { get; set; } = true;

        [Column(TypeName = "decimal(5,2)")]
        public decimal LowInventoryThreshold { get; set; } = 20.0m; // Percentage

    }
}
