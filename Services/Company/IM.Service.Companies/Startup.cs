
using CommonServices.RepositoryService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Polly;

using System;
using IM.Service.Companies.Clients;
using IM.Service.Companies.DataAccess;
using IM.Service.Companies.DataAccess.Entities;
using IM.Service.Companies.DataAccess.Repository;
using IM.Service.Companies.Services.DtoServices;
using IM.Service.Companies.Services.RabbitServices;
using IM.Service.Companies.Settings;

namespace IM.Service.Companies
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;
        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));

            services.AddDbContext<DatabaseContext>(provider =>
            {
                provider.UseLazyLoadingProxies();
                provider.UseNpgsql(Configuration["ServiceSettings:ConnectionStrings:Db"]);
            });

            services.AddControllers();

            services.AddHttpClient<CompanyPricesClient>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));

            services.AddScoped<IRepository<Company>, CompanyRepository>();
            services.AddScoped<IRepository<StockSplit>, StockSplitRepository>();
            services.AddScoped(typeof(RepositorySet<>));

            services.AddScoped<DtoCompanyManager>();
            services.AddScoped<DtoStockSplitManager>();
            services.AddScoped<RabbitCrudService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
