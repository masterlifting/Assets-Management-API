using CommonServices.RepositoryService;

using IM.Gateway.Companies.DataAccess.Entities;

using System.Collections.Generic;
using System.Linq;

namespace IM.Gateway.Companies.DataAccess.Repository
{
    public class CompanyRepository : IRepository<Entities.Company>
    {
        private readonly DatabaseContext context;
        public CompanyRepository(DatabaseContext context) => this.context = context;

        public bool TryCheckEntity(Entities.Company entity, out Entities.Company? result)
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
        public bool TryCheckEntities(IEnumerable<Entities.Company> entities, out Entities.Company[] result)
        {
            var checkedEntities = entities.Where(x => x?.Ticker is not null).ToArray();

            foreach (var company in checkedEntities)
                company.Ticker = company.Ticker.ToUpperInvariant().Trim();

            result = checkedEntities;
            return true;
        }
        public Entities.Company? GetIntersectedContextEntity(Entities.Company entity) => context.Companies.Find(entity.Ticker);
        public IQueryable<Entities.Company>? GetIntersectedContextEntities(IEnumerable<Entities.Company> entities)
        {
            var names = entities.Select(y => y.Ticker).ToArray();
            return context.Companies.Where(x => names.Contains(x.Ticker));
        }
        public bool UpdateEntity(Entities.Company oldResult, Entities.Company newResult)
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
