using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.PaymentDTOs
{
    public class CreatePaymentLinkRequest
    {
        public string productName { get; set; }
        public string description { get; set; }
        
        public int price { get; set; }
   public string returnUrl { get; set; }
   public string cancelUrl { get; set; }
    }
}
