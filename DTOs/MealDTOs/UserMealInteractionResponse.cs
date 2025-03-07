using BusinessObjects.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.MealDTOs
{
    public class UserMealInteractionResponse
    {
        public string Id { get; set; }
        public MealResponse Meal { get; set; }
        public InteractionType InteractionType { get; set; }
        public DateTime LastInteractionTime { get; set; }
    }
}
