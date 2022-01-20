using DataSetter.Clients;
using DataSetter.DataAccess.Company;
using DataSetter.DataAccess.CompanyData;
using DataSetter.Settings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Polly;

using System;

namespace DataSetter;

public class Startup
{
    public Startup(IConfiguration configuration) => Configuration = configuration;
    private IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));
            
        services.AddMemoryCache();

        services.AddDbContext<CompanyDatabaseContext>(provider =>
            provider.UseNpgsql(Configuration["ServiceSettings:ConnectionStrings:Company"]));
        services.AddDbContext<CompanyDataDatabaseContext>(provider =>
            provider.UseNpgsql(Configuration["ServiceSettings:ConnectionStrings:CompanyData"]));

        services.AddControllers();

        services.AddHttpClient<CompanyDataClient>()
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
            .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));
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