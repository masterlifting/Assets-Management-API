using System.Collections.Generic;
using IM.Service.Broker.Data.DataAccess.Entities;

using Microsoft.AspNetCore.Http;

using System.IO;
using System.Text.RegularExpressions;
using static IM.Service.Broker.Data.Enums;

namespace IM.Service.Broker.Data.Services.DataServices.Reports;

public static class ReportHelper
{
    public static Brokers? GetBroker(string fileName)
    {
        Brokers? result = null;

        foreach (var (pattern, broker) in brokerMatcher)
        {
            var match = Regex.Match(fileName, pattern, RegexOptions.IgnoreCase);

            if (!match.Success) 
                continue;
            
            result = broker;
            break;
        }

        return result;
    }
    
    private static readonly Dictionary<string, Brokers> brokerMatcher = new()
    {
        { "^B_k-(.+)_ALL(.+).xls$", Brokers.Bcs }
    };
}