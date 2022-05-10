using IM.Service.Common.Net.Helpers;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Recommendations.Clients;
using IM.Service.Recommendations.Domain.DataAccess;
using IM.Service.Recommendations.Domain.DataAccess.RepositoryHandlers;
using IM.Service.Recommendations.Domain.Entities;
using IM.Service.Recommendations.Settings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Polly;

using System;
using IM.Service.Recommendations.Services.Background;
using IM.Service.Recommendations.Services.Http;
using IM.Service.Recommendations.Services.RabbitMq;

namespace IM.Service.Recommendations;

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
            provider.UseLazyLoadingProxies();
            provider.UseNpgsql(Configuration["ServiceSettings:ConnectionStrings:Db"]);
        });

        services.AddControllers();

        services.AddHttpClient<MarketClient>()
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
            .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));
        services.AddHttpClient<PortfolioClient>()
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
            .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));

        services.AddControllers().AddJsonOptions(x =>
        {
            x.JsonSerializerOptions.Converters.Add(new JsonHelper.TimeOnlyConverter());
            x.JsonSerializerOptions.Converters.Add(new JsonHelper.DateOnlyConverter());
        });

        services.AddScoped(typeof(Repository<>));
        services.AddScoped<RepositoryHandler<Company>, CompanyRepositoryHandler>();

        services.AddScoped<PurchaseApi>();
        services.AddScoped<SaleApi>();

        services.AddScoped<RabbitActionService>();
        services.AddHostedService<RabbitBackgroundService>();
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