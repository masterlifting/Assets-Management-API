﻿using CommonServices.Attributes;
using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.GatewayCompanies
{
    public class StockSplitPostDto : PriceIdentity
    {
        [NotZero(nameof(Value))]
        public int Value { get; init; }
    }
}
