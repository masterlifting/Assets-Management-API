namespace IM.Gateways.Web.Companies.Api.Models.Dto.State
{
    public class ReportSourceModel
    {
        public int Id { get; set; }
        public string Value { get; set; } = null!;
        public bool IsActive { get; set; }
        public byte ReportSourceTypeId { get; set; }
    }
}
