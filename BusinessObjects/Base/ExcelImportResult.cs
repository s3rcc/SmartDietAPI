namespace BusinessObjects.Base
{
    public class ExcelImportResult<T>
    {
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public List<ExcelImportError> Errors { get; set; } = new();
    }

    public class ExcelImportError
    {
        public int RowNumber { get; set; }
        public string RowData { get; set; }
        public string ErrorMessage { get; set; }
    }
} 