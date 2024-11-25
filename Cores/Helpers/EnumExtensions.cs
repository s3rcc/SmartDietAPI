using BusinessObjects.FixedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cores.Helpers
{
    public static class EnumExtensions
    {
        public static string GetStorageTemperature(this StorageLocation location)
        {
            return location switch
            {
                StorageLocation.RoomTemperature => "20-25°C / 68-77°F",
                StorageLocation.Refrigerator => "0-4°C / 32-39°F",
                StorageLocation.Freezer => "-18°C / 0°F or below",
                _ => "Unknown temperature range"
            };
        }

        public static string GetShelfLife(this StorageLocation location, FoodCategory category)
        {
            return (location, category) switch
            {
                (StorageLocation.Refrigerator, FoodCategory.Meat) => "3-5 days",
                (StorageLocation.Refrigerator, FoodCategory.Dairy) => "5-7 days",
                (StorageLocation.Freezer, FoodCategory.Meat) => "4-6 months",
                (StorageLocation.RoomTemperature, FoodCategory.Grains) => "6-12 months",
                // Add more combinations as needed
                _ => "Varies based on specific item"
            };
        }
    }
}
