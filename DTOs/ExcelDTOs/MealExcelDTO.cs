using BusinessObjects.Base;

namespace DTOs.ExcelDTOs
{
    public class MealExcelDTO
    {
        [Column(1)] // Column A
        public string Name { get; set; }

        [Column(4)] // Column D
        public string Description { get; set; }

        [Column(9)] // Column I
        public string Image { get; set; }
    }
}