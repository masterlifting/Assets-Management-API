
using CommonServices.RepositoryService;
using IM.Gateway.Companies.DataAccess.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Polly;

using System;
using IM.Gateway.Companies.Clients;
using IM.Gateway.Companies.DataAccess;
using IM.Gateway.Companies.DataAccess.Repository;
using IM.Gateway.Companies.Services.CompanyServices;
using IM.Gateway.Companies.Services.DtoServices;
using IM.Gateway.Companies.Services.RabbitServices;
using IM.Gateway.Companies.Settings;

namespace IM.Gateway.Companies
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));

            services.AddDbContext<DatabaseContext>(provider =>
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