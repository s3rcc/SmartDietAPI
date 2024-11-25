using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Base;

namespace BusinessObjects.Entity
{
    public class NutrientCategory : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Food> Foods { get; set; }
    }
}
