using BusinessObjects.Entity;
using BusinessObjects.FixedData;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.DishDTOs
{
    public class DishDTO
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? Image { get; set; }
        public IFormFile? Video { get; set; }
        [Required]
        public string Instruction { get; set; }
        [Required]
        public int PrepTimeMinutes { get; set; }
        [Required]
        public int CookingTimeMinutes { get; set; }
        public RegionType RegionType { get; set; }
        public DietType? DietType { get; set; }
    }
}
