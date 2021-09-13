using System.Threading.Tasks;

namespace IM.Services.Company.Analyzer.Services.CalculatorServices.Interfaces
{
    public interface IAnalyzerCalculator<in T> where T : class
    {
        Task<bool> CalculateAsync();
        Task<bool> IsSetCalculatingStatusAsync(T[] collection);
    }
}
