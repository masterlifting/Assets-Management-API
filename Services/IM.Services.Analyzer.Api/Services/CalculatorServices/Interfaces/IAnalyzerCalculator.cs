using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.CalculatorServices.Interfaces
{
    public interface IAnalyzerCalculator<T> where T : class
    {
        Task CalculateAsync();
        Task<bool> IsSetCalculatingStatusAsync(T[] collection);
    }
}
