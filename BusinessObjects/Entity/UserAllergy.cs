using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Base;

namespace BusinessObjects.Entity
{
    public class UserAllergy : BaseEntity
    {
        public string SmartDietUserId { get; set; }
        public SmartDietUser SmartDietUser { get; set; }
        public string FoodId { get; set; }
        public Food Food { get; set; }
    }
}
