using System;
using System.Threading.Tasks;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces
{
    public interface IAnalyzerCalculator<in T> where T : class
    {
        Task<bool> CalculateAsync();
        Task<bool> CalculateAsync(DateTime dateStart);
        Task<bool> IsSetCalculatingStatusAsync(T[] collection, string info);
    }
}
