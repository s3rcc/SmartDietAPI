using BusinessObjects.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.FridgeDTOs
{
    public class FridgeRespose
    {
        public string Id { get; set; }
        public string? FridgeModel { get; set; }
        public string? FridgeLocation { get; set; }
    }
}
