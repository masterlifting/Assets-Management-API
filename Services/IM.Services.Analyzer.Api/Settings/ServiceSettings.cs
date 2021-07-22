using IM.Services.Analyzer.Api.Settings.Calculator;
using IM.Services.Analyzer.Api.Settings.Connection;
using IM.Services.Analyzer.Api.Settings.Mq;

namespace IM.Services.Analyzer.Api.Settings
{
    public class ServiceSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
        public MqSettings MqSettings { get; set; } = null!;
        public CalculatorSettings CalculatorSettings { get; set; } = null!;
    }
}
