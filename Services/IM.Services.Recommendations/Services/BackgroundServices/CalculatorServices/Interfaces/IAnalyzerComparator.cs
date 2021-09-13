﻿namespace IM.Services.Recommendations.Services.CalculatorServices.Interfaces
{
    public interface IAnalyzerComparator<T> where T : class
    {
        public T[] GetComparedSample();
    }
}