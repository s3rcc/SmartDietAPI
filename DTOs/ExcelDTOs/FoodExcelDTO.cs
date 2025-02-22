using BusinessObjects.Base;

namespace DTOs.ExcelDTOs
{
    public class FoodExcelDTO
    {
        [Column(1)]  
        public string Name { get; set; }

        [Column(7)]
        public int? ShelfLifeRoomTemp { get; set; }

        [Column(8)]
        public int? ShelfLifeRefrigerated { get; set; }

        [Column(9)]
        public int? ShelfLifeFrozen { get; set; }

        [Column(10)]  
        public string? Image { get; set; }
    }
}