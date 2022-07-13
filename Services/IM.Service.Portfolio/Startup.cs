using IM.Service.Shared.Helpers;
using IM.Service.Shared.SqlAccess;
using IM.Service.Portfolio.Clients;
using IM.Service.Portfolio.Domain.DataAccess;
using IM.Service.Portfolio.Domain.DataAccess.RepositoryHandlers;
using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Domain.Entities.Catalogs;
using IM.Service.Portfolio.Services.Background;
using IM.Service.Portfolio.Services.Data.Reports;
using IM.Service.Portfolio.Services.Http;
using IM.Service.Portfolio.Services.RabbitMq.Function.Processes;
using IM.Service.Portfolio.Services.RabbitMq.Sync.Processes;
using IM.Service.Portfolio.Settings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Polly;

using System;
using IM.Service.Portfolio.Services.Entity;
using IM.Service.Portfolio.Services.RabbitMq;

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
            x.JsonSerializerOptions.Converters.Add(new JsonHelper.TimeOnlyConverter());
            x.JsonSerializerOptions.Converters.Add(new JsonHelper.DateOnlyConverter());
        });

        services.AddScoped(typeof(Repository<>));
        services.AddScoped<RepositoryHandler<User>, UserRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Account>, AccountRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Provider>, ProviderRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Deal>, DealRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Expense>, ExpenseRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Income>, IncomeRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Event>, EventRepositoryHandler>();
        services.AddScoped<RepositoryHandler<EventType>, EventTypeRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Report>, ReportRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Asset>, AssetRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Derivative>, DerivativeRepositoryHandler>();

        services.AddScoped<ReportApi>();
        services.AddScoped<ReportGrabber>();

        services.AddScoped<AssetService>();
        services.AddScoped<DealService>();
        services.AddScoped<EventService>();
        services.AddScoped<ReportService>();

        services.AddTransient<AssetProcess>();
        services.AddTransient<DealProcess>();
        services.AddTransient<EventProcess>();
        services.AddTransient<ReportProcess>();

        services.AddSingleton<RabbitAction>();
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