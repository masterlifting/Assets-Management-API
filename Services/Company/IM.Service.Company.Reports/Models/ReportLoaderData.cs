using CommonServices.Models.Entity;

namespace IM.Service.Company.Reports.Models
{
    public class ReportLoaderData : ReportIdentity
    {
        public string SourceValue { get; }
        public ReportLoaderData(string sourceValue) => SourceValue = sourceValue;
    }
}
