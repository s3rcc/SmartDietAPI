using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.PaymentDTOs
{
    public class PaymentIsPaidResponse
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? SubscriptionId { get; set; }
        public string? SmartDietUserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
