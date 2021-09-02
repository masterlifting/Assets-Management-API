using CommonServices.Models;

using IM.Services.Analyzer.Api.Settings.Calculator;
using IM.Services.Analyzer.Api.Settings.Client;

namespace IM.Services.Analyzer.Api.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientSettings { get; set; } = null!;
        public CalculatorSettings CalculatorSettings { get; set; } = null!;
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
    }
}
