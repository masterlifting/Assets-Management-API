using CommonServices.RepositoryService;

using IM.Services.Companies.Prices.Api.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;

using static IM.Services.Companies.Prices.Api.Enums;

namespace IM.Services.Companies.Prices.Api.DataAccess.Repository
{
    public class TickerRepositoryHandler : IRepository<Ticker>
    {
        private readonly PricesContext context;
        public TickerRepositoryHandler(PricesContext context) => this.context = context;

        public bool TryCheckEntity(Ticker entity, out Ticker result)
        {
            if (context.SourceTypes.Find(entity.SourceTypeId) is null)
                entity.SourceTypeId = (byte)PriceSourceTypes.Default;

            result = entity;
            return true;
        }
        public bool TryCheckEntities(IEnumerable<Ticker> entities, out Ticker[] result)
        {
            result = entities.ToArray();
            
            var names = entities.Select(x => x.Name).ToArray();
            var correctNames = entities.Join(context.SourceTypes, x => x.SourceTypeId, y => y.Id, (x, _) => x.Name).ToArray();

            if (correctNames.Length != names.Length)
            {
                var incorrectedNames = names.Except(correctNames);
                var incorrectedEntities = entities.Join(incorrectedNames, x => x.Name, y => y, (x, _) => x).ToArray();

                for (int i = 0; i < incorrectedEntities.Length; i++)
                    incorrectedEntities[i].SourceTypeId = (byte)PriceSourceTypes.Default;

                result = entities.Join(correctNames, x => x.Name, y => y,(x,_) => x).Union(incorrectedEntities).ToArray();
            }

            return true;
        }
        public Ticker GetIntersectedContextEntity(Ticker entity) => context.Tickers.Find(entity.Name);
        public IQueryable<Ticker> GetIntersectedContextEntities(IEnumerable<Ticker> entities)
        {
            var names = entities.Select(y => y.Name).ToArray();
            return context.Tickers.Where(x => names.Contains(x.Name));
        }
        public bool UpdateEntity(Ticker oldResult, Ticker newResult)
        {
            var isCompare = (oldResult.Name == newResult.Name);

            if (isCompare)
                oldResult.SourceTypeId = newResult.SourceTypeId;

            return isCompare;
        }
    }
}
