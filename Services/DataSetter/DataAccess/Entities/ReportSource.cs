﻿using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class ReportSource
    {
        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
        public long CompanyId { get; set; }

        public virtual Company Company { get; set; } = null!;
    }
}