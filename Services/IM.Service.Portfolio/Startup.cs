using System;
using IM.Service.Common.Net.HttpServices.JsonConvertors;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Portfolio.Clients;
using IM.Service.Portfolio.DataAccess;
using IM.Service.Portfolio.DataAccess.Entities;
using IM.Service.Portfolio.DataAccess.Entities.Catalogs;
using IM.Service.Portfolio.DataAccess.Repositories;
using IM.Service.Portfolio.Services.BackgroundServices;
using IM.Service.Portfolio.Services.DataServices.Reports;
using IM.Service.Portfolio.Services.DtoServices;
using IM.Service.Portfolio.Services.MqServices;
using IM.Service.Portfolio.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;

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
        services.AddScoped<IRepositoryHandler<User>, UserRepository>();
        services.AddScoped<IRepositoryHandler<Account>, AccountRepository>();
        services.AddScoped<IRepositoryHandler<Broker>, BrokerRepository>();
        services.AddScoped<IRepositoryHandler<Event>, EventRepository>();
        services.AddScoped<IRepositoryHandler<Deal>, DealRepository>();
        services.AddScoped<IRepositoryHandler<Report>, ReportRepository>();
        services.AddScoped<IRepositoryHandler<UnderlyingAsset>, UnderlyingAssetRepository>();
        services.AddScoped<IRepositoryHandler<Derivative>, DerivativeRepository>();

        services.AddScoped<ReportDtoManager>();

        services.AddScoped<ReportGrabber>();
        services.AddScoped<ReportLoader>();

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