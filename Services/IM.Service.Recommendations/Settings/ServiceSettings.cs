using IM.Service.Shared.Models.Configuration;
using IM.Service.Recommendations.Settings.Sections;

namespace IM.Service.Recommendations.Settings;

public class ServiceSettings
{
    public ClientSettings ClientSettings { get; set; } = null!;
    public ConnectionStrings ConnectionStrings { get; set; } = null!;
    public SaleSettings SaleSettings { get; set; } = null!;
    public PurchaseSettings PurchaseSettings { get; set; } = null!;
}