using System;

namespace IM.Services.Companies.Prices.Api.Models.Client.MoexModels
{
    public class MoexHistoryPriceResultModel
    {
        public MoexHistoryPriceResultModel(MoexHistoryPriceData data, string ticker)
        {
            Data = data;
            Ticker = ticker;
        }

        public MoexHistoryPriceData Data { get; }
        public string Ticker { get; }
    }
}