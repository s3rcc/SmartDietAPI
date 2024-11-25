using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Base;

namespace BusinessObjects.Entity
{
    public class FoodAllergy : BaseEntity
    {
        public string FoodId { get; set; }
        public Food Food { get; set; }
        public string AllergenFoodId { get; set; }
        public Food AllergenFood { get; set; }
    }
}
