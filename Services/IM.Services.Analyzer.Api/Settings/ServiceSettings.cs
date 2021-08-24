using CommonServices.Models;

using IM.Services.Analyzer.Api.Settings.Calculator;

namespace IM.Services.Analyzer.Api.Settings
{
    public class ServiceSettings
    {
        public CalculatorSettings CalculatorSettings { get; set; } = null!;
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
    }
}
