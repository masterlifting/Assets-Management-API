namespace IM.Service.Company.Data.Models.Data
{
    public record ReportDataConfigModel
    {
        public string CompanyId { get; init; } = null!;
        public int Year { get; init; }
        public byte Quarter { get; init; }
        public string? SourceValue { get; init; }
    }
}
