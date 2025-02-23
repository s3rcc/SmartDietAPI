using BusinessObjects.Base;
using System;

namespace DTOs.ExcelDTOs
{
    public class DishExcelDTO
    {
        private static readonly Random _random = new();

        [Column(1)]
        public string Name { get; set; }

        [Column(4)]
        public string? Description { get; set; }

        //[Column(5)]
        public string Instruction { get; set; } = "Whoknow";

        public int PrepTimeMinutes { get; set; } = _random.Next(7, 78);
        public int CookingTimeMinutes { get; set; } = _random.Next(7, 78);

        [Column(9)]
        public string? Image { get; set; }
    }
}
