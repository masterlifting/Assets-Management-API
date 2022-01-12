
using System;

namespace IM.Service.Company.Data.Services.DataServices.StockSplits;

public class StockSplitGrabber : DataGrabber
{
    public StockSplitGrabber() : base(new(StringComparer.InvariantCultureIgnoreCase))
    {
    }
}