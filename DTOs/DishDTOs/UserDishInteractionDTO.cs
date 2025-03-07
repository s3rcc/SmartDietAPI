using BusinessObjects.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.DishDTOs
{
    public class UserDishInteractionDTO
    {
        public string DishId { get; set; }
        public InteractionType InteractionType { get; set; }
    }
}
