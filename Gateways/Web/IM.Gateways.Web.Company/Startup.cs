
using CommonServices.RepositoryService;
using IM.Gateways.Web.Company.DataAccess.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Polly;

using System;
using IM.Gateways.Web.Company.Clients;
using IM.Gateways.Web.Company.DataAccess;
using IM.Gateways.Web.Company.DataAccess.Repository;
using IM.Gateways.Web.Company.Services.CompanyServices;
using IM.Gateways.Web.Company.Services.DtoServices;
using IM.Gateways.Web.Company.Services.RabbitServices;
using IM.Gateways.Web.Company.Settings;

namespace IM.Gateways.Web.Company
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));

            services.AddDbContext<GatewaysContext>(provider =>
            {
                //todo: connection string to improve
                provider.UseNpgsql(Configuration["ServiceSettings:ConnectionStrings:Db"]);
            });

            services.AddControllers();

            services.AddScoped<IRepository<DataAccess.Entities.Company>, CompanyRepository>();
            services.AddScoped(typeof(RepositorySet<>));

            services.AddScoped<CompanyManager>();
            services.AddScoped<CompanyDtoAggregator>();
            services.AddScoped<RabbitCrudService>();

            services.AddHttpClient<PricesClient>()
               .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(10, retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp))))
               .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(10)));
            services.AddHttpClient<ReportsClient>()
               .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(10, retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp))))
               .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(10)));
            services.AddHttpClient<AnalyzerClient>()
               .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(10, retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp))))
               .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(10)));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
