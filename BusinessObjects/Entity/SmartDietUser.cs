using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entity
{
    public class SmartDietUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public UserProfile UserProfile { get; set; }
        public UserPreference? UserPreference { get; set; }
        //public ICollection<FavoriteDish>? FavoriteDishes { get; set; }
        public ICollection<FavoriteMeal>? FavoriteMeals { get; set; }
        public ICollection<UserAllergy>? UserAllergies { get; set; }
        public ICollection<Fridge>? Fridges { get; set; }
        public ICollection<MealRating>? MealRatings { get; set; }
    }
}
