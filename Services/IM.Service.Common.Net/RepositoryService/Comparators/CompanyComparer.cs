﻿
using IM.Service.Common.Net.Models.Entity;

using System;
using System.Collections.Generic;

namespace IM.Service.Common.Net.RepositoryService.Comparators;

public class CompanyComparer<T> : IEqualityComparer<T> where T : Company
{
    public bool Equals(T? x, T? y) => string.Equals(x!.Name, y!.Name, StringComparison.InvariantCultureIgnoreCase);
    public int GetHashCode(T obj) => obj.Name.GetHashCode();
}