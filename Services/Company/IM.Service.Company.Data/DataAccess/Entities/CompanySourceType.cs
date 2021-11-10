namespace IM.Service.Company.Data.DataAccess.Entities
{
    public class CompanySourceType
    {
        public virtual Company Company { get; set; } = null!;
        public string CompanyId { get; set; } = null!;

        public virtual SourceType SourceType { get; set; } = null!;
        public byte SourceTypeId { get; set; }

        public string? Value { get; set; }
    }
}
