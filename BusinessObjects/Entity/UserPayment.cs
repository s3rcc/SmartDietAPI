using BusinessObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entity
{
    public class UserPayment : BaseEntity
    {
        public string description { get; set; }
        public float Amount { get; set; } 
        public string? PaymentMethod { get; set; } 
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string? PaymentStatus { get; set; }
        public string SmartDietUserId { get; set; }
        public SmartDietUser SmartDietUser { get; set; }
    }
}
