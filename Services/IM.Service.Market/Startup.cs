using IM.Service.Common.Net.RepositoryService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Polly;

using System;
using IM.Service.Common.Net.HttpServices.JsonConvertors;
using IM.Service.Market.Clients;
using IM.Service.Market.DataAccess;
using IM.Service.Market.DataAccess.Entities;
using IM.Service.Market.DataAccess.Entities.ManyToMany;
using IM.Service.Market.DataAccess.Repositories;
using IM.Service.Market.Services.BackgroundServices;
using IM.Service.Market.Services.DataServices.Prices;
using IM.Service.Market.Services.DataServices.Reports;
using IM.Service.Market.Services.DataServices.StockSplits;
using IM.Service.Market.Services.DataServices.StockVolumes;
using IM.Service.Market.Services.DtoServices;
using IM.Service.Market.Services.MqServices;
using IM.Service.Market.Settings;

namespace IM.Service.Market;

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

        services.AddControllers().AddJsonOptions(x =>
            {
                x.JsonSerializerOptions.Converters.Add(new TimeOnlyConverter());
                x.JsonSerializerOptions.Converters.Add(new DateOnlyConverter());
            });

        services.AddHttpClient<TdAmeritradeClient>()
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
            .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));
        services.AddHttpClient<MoexClient>()
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
            .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));
        services.AddHttpClient<InvestingClient>()
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
            .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));

        services.AddScoped<PriceGrabber>();
        services.AddScoped<ReportGrabber>();
        services.AddScoped<StockSplitGrabber>();
        services.AddScoped<StockVolumeGrabber>();

        services.AddScoped<PriceLoader>();
        services.AddScoped<ReportLoader>();
        services.AddScoped<StockSplitLoader>();
        services.AddScoped<StockVolumeLoader>();

        services.AddScoped<CompanyDtoManager>();
        services.AddScoped<PricesDtoManager>();
        services.AddScoped<ReportsDtoManager>();
        services.AddScoped<StockSplitsDtoManager>();
        services.AddScoped<StockVolumesDtoManager>();

        services.AddScoped(typeof(Repository<>));
        services.AddScoped<IRepositoryHandler<Company>, CompanyRepository>();
        services.AddScoped<IRepositoryHandler<Industry>, IndustryRepository>();
        services.AddScoped<IRepositoryHandler<Sector>, SectorRepository>();
        services.AddScoped<IRepositoryHandler<Price>, PriceRepository>();
        services.AddScoped<IRepositoryHandler<Report>, ReportRepository>();
        services.AddScoped<IRepositoryHandler<StockSplit>, StockSplitRepository>();
        services.AddScoped<IRepositoryHandler<StockVolume>, StockVolumeRepository>();
        services.AddScoped<IRepositoryHandler<CompanySource>, CompanySourceRepository>();

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