namespace IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces
{
    public interface IAnalyzerComparator<T> where T : class
    {
        public T[] GetComparedSample();
    }
}
