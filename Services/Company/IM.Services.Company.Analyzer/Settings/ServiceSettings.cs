using CommonServices.Models;
using IM.Services.Company.Analyzer.Settings.Calculator;
using IM.Services.Company.Analyzer.Settings.Client;

namespace IM.Services.Company.Analyzer.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientSettings { get; set; } = null!;
        public CalculatorSettings CalculatorSettings { get; set; } = null!;
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
    }
}
