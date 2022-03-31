using IM.Service.Common.Net.HttpServices.JsonConvertors;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Clients;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.RepositoryHandlers;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.Catalogs;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.Background;
using IM.Service.Market.Services.Calculations;
using IM.Service.Market.Services.DataLoaders;
using IM.Service.Market.Services.DataLoaders.Floats;
using IM.Service.Market.Services.DataLoaders.Prices;
using IM.Service.Market.Services.DataLoaders.Reports;
using IM.Service.Market.Services.DataLoaders.Splits;
using IM.Service.Market.Services.Mq;
using IM.Service.Market.Services.RestApi;
using IM.Service.Market.Services.RestApi.Common;
using IM.Service.Market.Services.RestApi.Common.Interfaces;
using IM.Service.Market.Services.RestApi.Mappers;
using IM.Service.Market.Services.RestApi.Mappers.Interfaces;
using IM.Service.Market.Settings;
using Microsoft.EntityFrameworkCore;

using Polly;

namespace IM.Service.Market;

public class Startup
{
    private IConfiguration Configuration { get; }
    public Startup(IConfiguration configuration) => Configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));

        services.AddMemoryCache();

        services.AddDbContextPool<DatabaseContext>(provider =>
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

        services.AddScoped<IMapperRead<Report, ReportGetDto>, MapperReport>();
        services.AddScoped<IMapperRead<Price, PriceGetDto>, MapperPrice>();
        services.AddScoped<IMapperRead<Float, FloatGetDto>, MapperFloat>();
        services.AddScoped<IMapperRead<Split, SplitGetDto>, MapperSplit>();
        services.AddScoped<IMapperWrite<Report, ReportPostDto>, MapperReport>();
        services.AddScoped<IMapperWrite<Price, PricePostDto>, MapperPrice>();
        services.AddScoped<IMapperWrite<Float, FloatPostDto>, MapperFloat>();
        services.AddScoped<IMapperWrite<Split, SplitPostDto>, MapperSplit>();

        services.AddScoped<IRestQueryService<Coefficient>, RestQueryQuarterService<Coefficient>>();
        services.AddScoped<IRestQueryService<Dividend>, RestQueryDateService<Dividend>>();
        services.AddScoped<IRestQueryService<Report>,RestQueryQuarterService<Report>>();
        services.AddScoped<IRestQueryService<Price>, RestQueryDateService<Price>>();
        services.AddScoped<IRestQueryService<Float>, RestQueryDateService<Float>>();
        services.AddScoped<IRestQueryService<Split>, RestQueryDateService<Split>>();

        services.AddScoped<CompanyRestApi>();
        services.AddScoped<CompanySourceRestApi>();
        services.AddScoped<RatingRestApi>();
        services.AddScoped<RestApiRead<Report, ReportGetDto>>();
        services.AddScoped<RestApiRead<Price, PriceGetDto>>();
        services.AddScoped<RestApiRead<Float, FloatGetDto>>();
        services.AddScoped<RestApiRead<Split, SplitGetDto>>();
        //services.AddScoped<RestApiRead<Dividend, DividendGetDto>>();
        //services.AddScoped<RestApiRead<Coefficient, CoefficientGetDto>>();
        services.AddScoped<RestApiWrite<Report, ReportPostDto>>();
        services.AddScoped<RestApiWrite<Price, PricePostDto>>();
        services.AddScoped<RestApiWrite<Float, FloatPostDto>>();
        services.AddScoped<RestApiWrite<Split, SplitPostDto>>();
        //services.AddScoped<RestApiWrite<Dividend, DividendPostDto>>();
        //services.AddScoped<RestApiWrite<Coefficient, CoefficientPostDto>>();

        services.AddScoped<IDataLoader, PriceLoader>();
        services.AddScoped<IDataLoader, ReportLoader>();
        services.AddScoped<IDataLoader, FloatLoader>();
        services.AddScoped<IDataLoader, SplitLoader>();

        services.AddTransient<CoefficientService>();
        services.AddTransient<PriceService>();
        services.AddTransient<RatingCalculator>();

        services.AddScoped(typeof(Repository<>));
        services.AddScoped<IRepositoryHandler<Company>, CompanyRepositoryHandler>();
        services.AddScoped<IRepositoryHandler<CompanySource>, CompanySourceRepositoryHandler>();
        services.AddScoped<IRepositoryHandler<Industry>, IndustryRepositoryHandler>();
        services.AddScoped<IRepositoryHandler<Sector>, SectorRepositoryHandler>();
        services.AddScoped<IRepositoryHandler<Price>, PriceRepositoryHandler>();
        services.AddScoped<IRepositoryHandler<Report>, ReportRepositoryHandler>();
        services.AddScoped<IRepositoryHandler<Float>, FloatRepositoryHandler>();
        services.AddScoped<IRepositoryHandler<Split>, SplitRepositoryHandler>(); 
        services.AddScoped<IRepositoryHandler<Dividend>, DividendRepositoryHandler>();
        services.AddScoped<IRepositoryHandler<Coefficient>, CoefficientRepositoryHandler>();

        services.AddSingleton<RabbitActionService>();
        services.AddHostedService<RabbitBackgroundService>();
        services.AddSingleton<RatingComparator>();
        services.AddHostedService<RatingBackgroundService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment()) 
            app.UseDeveloperExceptionPage();

        app.UseRouting();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}