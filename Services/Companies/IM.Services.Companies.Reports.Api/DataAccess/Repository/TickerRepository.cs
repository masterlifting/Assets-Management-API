using CommonServices.RepositoryService;

using IM.Services.Companies.Reports.Api.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;

using static IM.Services.Companies.Reports.Api.Enums;

namespace IM.Services.Companies.Reports.Api.DataAccess.Repository
{
    public class TickerRepository : IRepository<Ticker>
    {
        private readonly ReportsContext context;
        public TickerRepository(ReportsContext context) => this.context = context;

        public bool TryCheckEntity(Ticker entity, out Ticker result)
        {
            if (context.SourceTypes.Find(entity.SourceTypeId) is null)
                entity.SourceTypeId = (byte)ReportSourceTypes.Default;

            result = entity;
            return true;
        }
        public bool TryCheckEntities(IEnumerable<Ticker> entities, out Ticker[] result)
        {
            result = entities.ToArray();
            
            var correctNames = entities.Join(context.SourceTypes, x => x.SourceTypeId, y => y.Id, (x, _) => x.Name).ToArray();
            var names = entities.Select(x => x.Name).ToArray();

            if (correctNames.Length != names.Length)
            {
                var incorrectedNames = names.Except(correctNames);
                var incorrectedEntities = entities.Join(incorrectedNames, x => x.Name, y => y, (x, _) => x).ToArray();

                for (int i = 0; i < incorrectedEntities.Length; i++)
                    incorrectedEntities[i].SourceTypeId = (byte)ReportSourceTypes.Default;

                result = entities.Join(correctNames, x => x.Name, y => y, (x, _) => x).Union(incorrectedEntities).ToArray();
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
