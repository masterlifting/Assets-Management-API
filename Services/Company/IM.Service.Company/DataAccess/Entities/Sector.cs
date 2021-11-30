﻿using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Company.DataAccess.Entities;

public class Sector : CommonEntityType
{
    public virtual IEnumerable<Industry>? Industries { get; set; }
}