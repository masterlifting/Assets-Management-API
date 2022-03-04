using System;
using System.Linq.Expressions;
using IM.Service.Common.Net.Models.Entity.Interfaces;

namespace IM.Service.Common.Net.RepositoryService.Filters;

public interface IFilter<T> where T : IPeriod
{
    Expression<Func<T, bool>> Expression { get; set; }
}