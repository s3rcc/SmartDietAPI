using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Base;

namespace BusinessObjects.Entity
{
    public class FavoriteDish : BaseEntity
    {
        public string SmartDietUserId { get; set; }
        public SmartDietUser SmartDietUser { get; set; }
        public string DishId { get; set; }
        public Dish Dish { get; set; }
    }
}
