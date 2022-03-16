using IM.Service.Common.Net.HttpServices.JsonConvertors;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.MarketData.Clients;
using IM.Service.MarketData.Domain.DataAccess;
using IM.Service.MarketData.Domain.DataAccess.RepositoryHandlers;
using IM.Service.MarketData.Domain.Entities;
using IM.Service.MarketData.Domain.Entities.Catalogs;
using IM.Service.MarketData.Domain.Entities.ManyToMany;
using IM.Service.MarketData.Services.Background;
using IM.Service.MarketData.Services.DataLoaders;
using IM.Service.MarketData.Services.DataLoaders.Floats;
using IM.Service.MarketData.Services.DataLoaders.Prices;
using IM.Service.MarketData.Services.DataLoaders.Reports;
using IM.Service.MarketData.Services.DataLoaders.Splits;
using IM.Service.MarketData.Services.Mq;
using IM.Service.MarketData.Services.RestApi;
using IM.Service.MarketData.Services.RestApi.Common;
using IM.Service.MarketData.Services.RestApi.Common.Interfaces;
using IM.Service.MarketData.Services.RestApi.Mappers;
using IM.Service.MarketData.Services.RestApi.Mappers.Interfaces;
using IM.Service.MarketData.Settings;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace IM.Service.MarketData;

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


        services.AddScoped<IMapperRead<Report,ReportGetDto>>();
        services.AddScoped<IMapperWrite<Report, ReportGetDto>>();
        services.AddScoped<MapperReport>();

        services.AddScoped<IRestQueryService<Report>,RestQueryQuarterService<Report>>();

        services.AddScoped<IDataLoader, DataLoader<Price>>();
        services.AddScoped<IDataLoader, DataLoader<Report>>();
        services.AddScoped<IDataLoader, DataLoader<Float>>();
        services.AddScoped<IDataLoader, DataLoader<Split>>();

        services.AddScoped<PriceLoader>();
        services.AddScoped<ReportLoader>();
        services.AddScoped<SplitLoader>();
        services.AddScoped<FloatLoader>();

        services.AddScoped<CompanyApi>();
        services.AddScoped<RatingApi>();

        services.AddScoped(typeof(Repository<>));
        services.AddScoped<IRepositoryHandler<Company>, CompanyRepositoryHandler>();
        services.AddScoped<IRepositoryHandler<Industry>, IndustryRepositoryHandler>();
        services.AddScoped<IRepositoryHandler<Sector>, SectorRepositoryHandler>();
        services.AddScoped<IRepositoryHandler<Price>, PriceRepositoryHandler>();
        services.AddScoped<IRepositoryHandler<Report>, ReportRepositoryHandler>();
        services.AddScoped<IRepositoryHandler<Split>, SplitRepositoryHandler>();
        services.AddScoped<IRepositoryHandler<Float>, FloatRepositoryHandler>();
        services.AddScoped<IRepositoryHandler<CompanySource>, CompanySourceRepositoryHandler>();

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