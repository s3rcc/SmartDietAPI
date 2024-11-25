using BusinessObjects.Entity;
using BusinessObjects.FixedData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.FoodDTOs
{
    public class FoodResponse
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string StorageGuidelines { get; set; }

        public int? ShelfLifeRoomTemp { get; set; }

        // Average shelf life in days in refrigerator
        public int? ShelfLifeRefrigerated { get; set; }

        // Average shelf life in days in freezer
        public int? ShelfLifeFrozen { get; set; }

        public PreservationType? PreservationType { get; set; }

        public FoodCategory Category { get; set; }

        public string? Image { get; set; }

        // Navigation properties
        public ICollection<NutrientCategoryResponse> NutrientCategories { get; set; } = new List<NutrientCategoryResponse>();
        public ICollection<FoodAllergyResponse> FoodAllergies { get; set; } = new List<FoodAllergyResponse>();

        public DateTime CreatedTime { get; set; }

        public DateTime? LastUpdatedTime { get; set; }

        public DateTime? DeletedTime { get; set; }
    }
}
