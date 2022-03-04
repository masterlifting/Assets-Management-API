namespace IM.Service.Data.Services.DataFounders.Splits;

public class StockSplitGrabber : DataGrabber
{
    public StockSplitGrabber() : base(new(StringComparer.InvariantCultureIgnoreCase))
    {
    }
}