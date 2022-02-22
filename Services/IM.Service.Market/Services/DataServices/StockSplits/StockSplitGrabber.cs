
using System;

namespace IM.Service.Market.Services.DataServices.StockSplits;

public class StockSplitGrabber : DataGrabber
{
    public StockSplitGrabber() : base(new(StringComparer.InvariantCultureIgnoreCase))
    {
    }
}