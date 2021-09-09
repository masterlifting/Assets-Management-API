
using CommonServices.RepositoryService;

using IM.Gateways.Web.Companies.Api.Clients;
using IM.Gateways.Web.Companies.Api.DataAccess;
using IM.Gateways.Web.Companies.Api.DataAccess.Entities;
using IM.Gateways.Web.Companies.Api.DataAccess.Repository;
using IM.Gateways.Web.Companies.Api.Services.CompanyServices;
using IM.Gateways.Web.Companies.Api.Services.DtoServices;
using IM.Gateways.Web.Companies.Api.Services.RabbitServices;
using IM.Gateways.Web.Companies.Api.Settings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Polly;

using System;

namespace IM.Gateways.Web.Companies.Api
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

            services.AddScoped<IRepository<Company>, CompanyRepository>();
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
