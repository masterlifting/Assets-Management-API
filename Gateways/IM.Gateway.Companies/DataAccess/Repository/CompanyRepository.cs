using CommonServices.RepositoryService;

using IM.Gateway.Companies.DataAccess.Entities;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Gateway.Companies.DataAccess.Repository
{
    public class CompanyRepository : IRepository<Company>
    {
        private readonly DatabaseContext context;
        public CompanyRepository(DatabaseContext context) => this.context = context;

        public async Task<(bool trySuccess, Company? checkedEntity)> TryCheckEntityAsync(Company entity)
        {
            entity.Ticker = entity.Ticker.ToUpperInvariant().Trim();
            return await Task.FromResult((true, entity));
        }
        public async Task<(bool isSuccess, Company[] checkedEntities)> TryCheckEntitiesAsync(IEnumerable<Company> entities)
        {
            var arrayEntities = entities.ToArray();

            foreach (var entity in arrayEntities)
                entity.Ticker = entity.Ticker.ToUpperInvariant().Trim();

            return await Task.FromResult((true, arrayEntities));
        }
        public async Task<Company?> GetAlreadyEntityAsync(Company entity) => await context.Companies.FindAsync(entity.Ticker);
        public IQueryable<Company> GetAlreadyEntitiesQuery(IEnumerable<Company> entities)
        {
            var names = entities.Select(y => y.Ticker).ToArray();
            return context.Companies.Where(x => names.Contains(x.Ticker));
        }
        public bool IsUpdate(Company contextEntity, Company newEntity)
        {
            var isCompare = contextEntity.Ticker == newEntity.Ticker;

            if (isCompare)
            {
                contextEntity.Name = newEntity.Name;
                contextEntity.Description = newEntity.Description;
            }

            return isCompare;
        }
    }
}
