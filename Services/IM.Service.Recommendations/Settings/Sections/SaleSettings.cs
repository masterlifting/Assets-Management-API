using System;

namespace IM.Service.Recommendations.Settings.Sections;

public class SaleSettings
{
    public decimal[] DeviationPercents { get; set; } = Array.Empty<decimal>();
}