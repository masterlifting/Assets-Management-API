using IM.Service.Common.Net.RepositoryService;

using IM.Service.Company.Analyzer.Clients;
using IM.Service.Company.Analyzer.DataAccess;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using IM.Service.Company.Analyzer.Services.BackgroundServices;
using IM.Service.Company.Analyzer.Services.CalculatorServices;
using IM.Service.Company.Analyzer.Services.DtoServices;
using IM.Service.Company.Analyzer.Services.MqServices;
using IM.Service.Company.Analyzer.Settings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Polly;

using System;

namespace IM.Service.Company.Analyzer;

public class Startup
{
    public Startup(IConfiguration configuration) => Configuration = configuration;
    private IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));

        services.AddMemoryCache();

        services.AddDbContext<DatabaseContext>(provider =>
        {
            provider.UseNpgsql(Configuration["ServiceSettings:ConnectionStrings:Db"]);
        });

        services.AddControllers();

        services
            .AddHttpClient<CompanyDataClient>()
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
            .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));

        services.AddSingleton<AnalyzerService>();

        services.AddScoped<CalculatorReport>();
        services.AddScoped<CalculatorPrice>();
        services.AddScoped<CalculatorCoefficient>();

        services.AddScoped<RatingDtoManager>();

        services.AddScoped<IRepositoryHandler<DataAccess.Entities.Company>, CompanyRepository>();
        services.AddScoped<IRepositoryHandler<AnalyzedEntity>, AnalyzedEntityRepository>();
        services.AddScoped<IRepositoryHandler<Rating>, RatingRepository>();
        services.AddScoped(typeof(RepositorySet<>));

        services.AddSingleton<RabbitActionService>();

        services.AddHostedService<RabbitBackgroundService>();
        services.AddHostedService<CalculatorBackgroundService>();
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