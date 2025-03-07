using BusinessObjects.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.MealDTOs
{
    public class UserMealInteractionDTO
    {
        public string MealId { get; set; }
        public InteractionType InteractionType { get; set; }
    }
}
