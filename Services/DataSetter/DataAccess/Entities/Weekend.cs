using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class Weekend
    {
        public int Id { get; set; }
        public DateTime ExchangeWeekend { get; set; }
        public long ExchangeId { get; set; }

        public virtual Exchange Exchange { get; set; } = null!;
    }
}
