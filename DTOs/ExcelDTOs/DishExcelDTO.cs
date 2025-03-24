using BusinessObjects.Base;
using BusinessObjects.FixedData;
using System;

namespace DTOs.ExcelDTOs
{
    public class DishExcelDTO
    {

        [Column(1)]
        public string Name { get; set; }

        [Column(2)]
        public string Description { get; set; }

        [Column(3)]
        public string Image { get; set; }

        [Column(4)]
        public string Video { get; set; }

        [Column(5)]
        public string Instruction { get; set; }

        [Column(6)]
        public int PrepTimeMinutes { get; set; } 

        [Column(7)]
        public int CookingTimeMinutes { get; set; } 

        [Column(8)]
        public string Difficulty { get; set; }

        [Column(9)]
        public string RegionType { get; set; }

        [Column(10)]
        public string DietType { get; set; }
    }
}
