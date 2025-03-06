using BusinessObjects.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entity
{
    public class Subcription : BaseEntity
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public float Amount { get; set; }
        public string? SubscriptionType { get; set; } // Loại gói đăng ký (ví dụ: Basic, Premium, v.v.)
        public int MonthOfSubcription { get; set; } 
        public string? SubscriptionStatus { get; set; } // Trạng thái đăng ký (hoạt động, hết hạn, v.v.)

    }
}
