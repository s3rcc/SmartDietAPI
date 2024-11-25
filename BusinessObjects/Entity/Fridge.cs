using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Base;

namespace BusinessObjects.Entity
{
    public class Fridge : BaseEntity
    {
        [Required]
        public int SmartDietUserId { get; set; }
        public SmartDietUser SmartDietUser { get; set; }
        public string? FridgeModel { get; set; }
        public string? FridgeLocation { get; set; }
        public ICollection<FridgeItem>? FridgeItems { get; set; }
    }
}
