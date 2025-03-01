using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BusinessObjects.Base;

namespace BusinessObjects.Entity
{
    public class DishRating : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public string SmartDietUserId { get; set; }
        public SmartDietUser SmartDietUser { get; set; }
        public string DishId { get; set; }
        public Dish Dish { get; set; }
        public int Rating { get; set; } // 1-5 stars
        public string? Comment { get; set; }
        public DateTime RatingDate { get; set; }
    }
} 