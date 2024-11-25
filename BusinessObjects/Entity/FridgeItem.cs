
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.FixedData;
using BusinessObjects.Base;

namespace BusinessObjects.Entity
{
    public class FridgeItem : BaseEntity
    {
        [Required]
        public string FridgeId { get; set; }
        public Fridge Fridge { get; set; }


        [Required]
        public string FoodId { get; set; }
        public Food Food { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; }

        public MeasurementUnit Unit { get; set; }  // e.g., grams, pieces, packages

        [Required]
        public DateTime? PurchaseDate { get; set; }

        [Required]
        public DateTime? ExpirationDate { get; set; }

        [Required]
        public StorageLocation StorageLocation { get; set; }

        [StringLength(100)]
        public string Notes { get; set; }
    }
}
