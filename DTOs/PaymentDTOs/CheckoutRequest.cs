using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.PaymentDTOs
{
    public class CheckOutRequest
    {
        public string Name {  get; set; }
        public int Quantity { get; set; }

        public int Price { get; set; }
    }
}
