using IM.Services.Analyzer.Api.Settings.Calculator;
using IM.Services.Analyzer.Api.Settings.Rabbitmq;

namespace IM.Services.Analyzer.Api.Settings
{
    public class ServiceSettings
    {
        public CalculatorSettings CalculatorSettings { get; set; } = null!;
        public RabbitmqSettings RabbitmqSettings { get; set; } = null!;
    }
}
