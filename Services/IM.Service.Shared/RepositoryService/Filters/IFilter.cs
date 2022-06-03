using System;
using System.Linq.Expressions;
using IM.Service.Shared.Models.Entity.Interfaces;

namespace IM.Service.Shared.RepositoryService.Filters;

public interface IFilter<T> where T : IPeriod
{
    Expression<Func<T, bool>> Expression { get; set; }
}