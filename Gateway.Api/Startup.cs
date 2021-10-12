using Gateway.Api.Clients;
using Gateway.Api.HealthCheck;
using Gateway.Api.Settings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using Gateway.Api.DataAccess;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Gateway.Api
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

            services.AddHttpClient<CompaniesClient>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));
            services.AddHttpClient<CompanyPricesClient>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));
            services.AddHttpClient<CompanyReportsClient>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));
            services.AddHttpClient<CompanyAnalyzerClient>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));

            services.AddHealthChecks()
                .AddCheck<CompaniesHealthCheck>("Companies service health check")
                .AddCheck<CompanyReportsHealthCheck>("Company reports service health check")
                .AddCheck<CompanyPricesHealthCheck>("Company prices service health check")
                .AddCheck<CompanyAnalyzerHealthCheck>("Company analyzer service health check");
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
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
