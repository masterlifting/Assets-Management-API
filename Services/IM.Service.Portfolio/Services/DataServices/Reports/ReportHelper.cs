using System.Collections.Generic;
using System.Text.RegularExpressions;

using static IM.Service.Portfolio.Enums;

namespace IM.Service.Portfolio.Services.DataServices.Reports;

public static class ReportHelper
{
    public static bool TryGetBroker(string fileName, out Brokers result)
    {
        result = Brokers.Default;

        foreach (var (pattern, broker) in brokerMatcher)
        {
            var match = Regex.Match(fileName, pattern, RegexOptions.IgnoreCase);

            if (!match.Success) 
                continue;
            
            result = broker;
            break;
        }

        return result != Brokers.Default;
    }

    private static readonly Dictionary<string, Brokers> brokerMatcher = new()
    {
        { "^B_k-(.+)_ALL(.+).xls$", Brokers.Bcs }
    };
}