using CommonServices.RepositoryService;

using IM.Gateways.Web.Companies.Api.DataAccess.Entities;

using System.Collections.Generic;
using System.Linq;

namespace IM.Gateways.Web.Companies.Api.DataAccess.Repository
{
    public class CompanyRepository : IRepository<Company>
    {
        private readonly GatewaysContext context;
        public CompanyRepository(GatewaysContext context) => this.context = context;

        public bool TryCheckEntity(Company entity, out Company? result)
        {
            if (entity?.Ticker is null)
            {
                result = null;
                return false;
            }

            entity.Ticker = entity.Ticker.ToUpperInvariant().Trim();
            result = entity;
            return true;
        }
        public bool TryCheckEntities(IEnumerable<Company> entities, out Company[] result)
        {
            var checkedEntities = entities.Where(x => x?.Ticker is not null).ToArray();

            foreach (var t in checkedEntities)
                t.Ticker = t.Ticker.ToUpperInvariant().Trim();

            result = checkedEntities;
            return true;
        }
        public Company? GetIntersectedContextEntity(Company entity) => context.Companies.Find(entity.Ticker);
        public IQueryable<Company>? GetIntersectedContextEntities(IEnumerable<Company> entities)
        {
            var names = entities.Select(y => y.Ticker).ToArray();
            return context.Companies.Where(x => names.Contains(x.Ticker));
        }
        public bool UpdateEntity(Company oldResult, Company newResult)
        {
            var isCompare = oldResult.Ticker == newResult.Ticker;

            if (isCompare)
            {
                oldResult.Name = newResult.Name;
                oldResult.Description = newResult.Description;
            }

            return isCompare;
        }
    }
}
