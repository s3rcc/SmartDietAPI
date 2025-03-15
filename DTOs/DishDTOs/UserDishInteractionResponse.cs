using BusinessObjects.Enum;
using DTOs.MealDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.DishDTOs
{
    public class UserDishInteractionResponse
    {
        public string Id { get; set; }
        public DishResponse Dish { get; set; }
        public InteractionType InteractionType { get; set; }
        public DateTime InteractionDate { get; set; }
    }
}
