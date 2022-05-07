using IM.Service.Portfolio.Clients;
using IM.Service.Portfolio.Domain.DataAccess;
using IM.Service.Portfolio.Domain.DataAccess.RepositoryHandlers;
using IM.Service.Portfolio.Services.Background;
using IM.Service.Portfolio.Services.Data.Isins;
using IM.Service.Portfolio.Services.Data.Reports;
using IM.Service.Portfolio.Services.HttpRestApi;
using IM.Service.Portfolio.Services.RabbitMq;
using IM.Service.Portfolio.Settings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Polly;

using System;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Domain.Entities.Catalogs;
using static IM.Service.Common.Net.Helpers.JsonHelper;

namespace IM.Service.Portfolio;

public class Startup
{
    private IConfiguration Configuration { get; }
    public Startup(IConfiguration configuration) => Configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));

        services.AddMemoryCache();

        services.AddDbContext<DatabaseContext>(provider =>
        {
            provider.UseLazyLoadingProxies();
            provider.UseNpgsql(Configuration["ServiceSettings:ConnectionStrings:Db"]);
        });

        services.AddHttpClient<MoexClient>()
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
            .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));

        services.AddControllers().AddJsonOptions(x =>
        {
            x.JsonSerializerOptions.Converters.Add(new TimeOnlyConverter());
            x.JsonSerializerOptions.Converters.Add(new DateOnlyConverter());
        });

        services.AddScoped(typeof(Repository<>));
        services.AddScoped<RepositoryHandler<User>, UserRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Account>, AccountRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Broker>, BrokerRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Event>, EventRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Deal>, DealRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Report>, ReportRepositoryHandler>();
        services.AddScoped<RepositoryHandler<UnderlyingAsset>, UnderlyingAssetRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Derivative>, DerivativeRepositoryHandler>();

        services.AddScoped<ReportRestApi>();

        services.AddScoped<ReportGrabber>();
        services.AddScoped<ReportLoader>();

        services.AddTransient<MoexIsinService>();

        services.AddSingleton<RabbitActionService>();
        services.AddHostedService<RabbitBackgroundService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();

        app.UseRouting();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}