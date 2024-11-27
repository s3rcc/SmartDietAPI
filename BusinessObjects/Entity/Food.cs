using BusinessObjects.Base;
using BusinessObjects.FixedData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entity
{
    public class Food : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(200)]
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
        public ICollection<NutrientCategory> NutrientCategories { get; set; } = new List<NutrientCategory>();
        public ICollection<FoodAllergy> FoodAllergies { get; set; } = new List<FoodAllergy>();
    }
}
