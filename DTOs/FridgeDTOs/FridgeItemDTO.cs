using BusinessObjects.Entity;
using BusinessObjects.FixedData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.FridgeDTOs
{
    public class FridgeItemDTO
    {
        public string FridgeId { get; set; }
        public string FoodId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public StorageLocation StorageLocation { get; set; }
        public string Notes { get; set; }
    }
}
