using BusinessObjects.Base;
using BusinessObjects.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entity
{
    public class UserMealInteraction : BaseEntity
    {
        public string SmartDietUserId { get; set; }
        public SmartDietUser SmartDietUser { get; set; }
        public string MealId { get; set; }
        public Meal Meal { get; set; }
        public InteractionType InteractionType { get; set; }
        public DateTime LastInteractionTime { get; set; }
    }
}
