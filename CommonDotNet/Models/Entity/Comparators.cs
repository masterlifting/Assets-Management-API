using System;
using System.Collections.Generic;

namespace CommonServices.Models.Entity
{
    public class TickerComparer : IEqualityComparer<TickerIdentity>
    {
        public bool Equals(TickerIdentity? x, TickerIdentity? y) => x!.Name.Equals(y!.Name, StringComparison.InvariantCultureIgnoreCase);
        public int GetHashCode(TickerIdentity obj) => obj.Name.GetHashCode();
    }
    public class PriceComparer : IEqualityComparer<PriceIdentity>
    {
        public bool Equals(PriceIdentity? x, PriceIdentity? y) => (x!.TickerName, x.Date) == (y!.TickerName, y.Date);
        public int GetHashCode(PriceIdentity obj) => (obj.TickerName, obj.Date).GetHashCode();
    }

    public class ReportComparer : IEqualityComparer<ReportIdentity>
    {
        public bool Equals(ReportIdentity? x, ReportIdentity? y) => (x!.TickerName, x.Year, x.Quarter) == (y!.TickerName, y.Year, y.Quarter);
        public int GetHashCode(ReportIdentity obj) => (obj.TickerName, obj.Year, obj.Quarter).GetHashCode();
    }
}
