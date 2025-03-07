using BusinessObjects.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.SubcriptionDTOs
{
    public class SubcriptionRequest
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public float Amount { get; set; }
        public SubcriptionType? SubscriptionType { get; set; } 
        public int MonthOfSubcription { get; set; }
        public string? SubscriptionStatus { get; set; }
    }
}
