using CommonServices.Models;

using IM.Services.Recommendations.Settings.Calculator;
using IM.Services.Recommendations.Settings.Client;

namespace IM.Services.Recommendations.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientSettings { get; set; } = null!;
        public CalculatorSettings CalculatorSettings { get; set; } = null!;
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
    }
}
