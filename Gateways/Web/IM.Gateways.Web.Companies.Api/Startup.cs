using IM.Gateways.Web.Companies.Api.Clients;
using IM.Gateways.Web.Companies.Api.DataAccess;
using IM.Gateways.Web.Companies.Api.Services.Agregators.Implementations;
using IM.Gateways.Web.Companies.Api.Services.Agregators.Interfaces;
using IM.Gateways.Web.Companies.Api.Services.CompanyManagement.Implementations;
using IM.Gateways.Web.Companies.Api.Services.CompanyManagement.Interfaces;
using IM.Gateways.Web.Companies.Api.Services.RabbitMqManagement.Implementations;
using IM.Gateways.Web.Companies.Api.Services.RabbitMqManagement.Interfaces;
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
                provider.UseNpgsql(Configuration["ConnectionString"]);
            });

            services.AddControllers();

            services.AddScoped<ICompaniesManager, CompaniesManager>();
            services.AddScoped<ICompaniesDtoAgregator, CompaniesDtoAgregator>();
            services.AddScoped<IRabbitmqManager, RabbitmqManager>();

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
