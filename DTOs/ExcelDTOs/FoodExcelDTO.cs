using BusinessObjects.Base;

namespace DTOs.ExcelDTOs
{
public class FoodExcelDTO
    {
        [Column(1)]  
        public string Name { get; set; }
        
        [Column(2)]
        public string Description { get; set; }
        
        [Column(3)]
        public string StorageGuidelines { get; set; } = "Hiện tại không có hướng dẫn bảo quản";
        
        [Column(4)]
        public int? ShelfLifeRoomTemp { get; set; }
        
        [Column(5)]
        public int? ShelfLifeRefrigerated { get; set; }
        
        [Column(6)]
        public int? ShelfLifeFrozen { get; set; }
        
        [Column(7)]
        public string PreservationType { get; set; }
        
        [Column(8)]
        public string Category { get; set; }
        
        [Column(9)]  
        public string Image { get; set; }
    }
}