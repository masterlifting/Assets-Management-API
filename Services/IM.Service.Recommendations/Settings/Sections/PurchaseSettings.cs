using System;

namespace IM.Service.Recommendations.Settings.Sections;

public class PurchaseSettings
{
    public decimal[] DeviationPercents { get; set; } = Array.Empty<decimal>();
}