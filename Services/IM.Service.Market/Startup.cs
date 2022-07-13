using IM.Service.Market.Clients;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.RepositoryHandlers;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.Catalogs;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.Background;
using IM.Service.Market.Services.Background.Compute;
using IM.Service.Market.Services.Background.Load;
using IM.Service.Market.Services.Data;
using IM.Service.Market.Services.Data.Dividends;
using IM.Service.Market.Services.Data.Floats;
using IM.Service.Market.Services.Data.Prices;
using IM.Service.Market.Services.Data.Reports;
using IM.Service.Market.Services.Data.Splits;
using IM.Service.Market.Services.Entity;
using IM.Service.Market.Services.Http;
using IM.Service.Market.Services.Http.Common;
using IM.Service.Market.Services.Http.Common.Interfaces;
using IM.Service.Market.Services.Http.Mappers;
using IM.Service.Market.Services.Http.Mappers.Interfaces;
using IM.Service.Market.Services.RabbitMq;
using IM.Service.Market.Services.RabbitMq.Function.Processes;
using IM.Service.Market.Settings;
using IM.Service.Shared.SqlAccess;

using Microsoft.EntityFrameworkCore;

using Polly;

using static IM.Service.Shared.Helpers.JsonHelper;

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
            provider.UseNpgsql(Configuration["ServiceSettings:ConnectionStrings:Paviams"]);
        }, ServiceLifetime.Transient);

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

        services.AddScoped(typeof(Repository<>));
        services.AddScoped<RepositoryHandler<Company>, CompanyRepositoryHandler>();
        services.AddScoped<RepositoryHandler<CompanySource>, CompanySourceRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Industry>, IndustryRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Sector>, SectorRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Price>, PriceRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Report>, ReportRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Float>, FloatRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Split>, SplitRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Dividend>, DividendRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Coefficient>, CoefficientRepositoryHandler>();
        services.AddScoped<RepositoryHandler<Rating>, RatingRepositoryHandler>();

        services.AddScoped<IMapperRead<Report, ReportGetDto>, MapperReport>();
        services.AddScoped<IMapperWrite<Report, ReportPostDto>, MapperReport>();
        services.AddScoped<IMapperRead<Price, PriceGetDto>, MapperPrice>();
        services.AddScoped<IMapperWrite<Price, PricePostDto>, MapperPrice>();
        services.AddScoped<IMapperRead<Float, FloatGetDto>, MapperFloat>();
        services.AddScoped<IMapperWrite<Float, FloatPostDto>, MapperFloat>();
        services.AddScoped<IMapperRead<Split, SplitGetDto>, MapperSplit>();
        services.AddScoped<IMapperWrite<Split, SplitPostDto>, MapperSplit>();
        services.AddScoped<IMapperRead<Dividend, DividendGetDto>, MapperDividend>();
        services.AddScoped<IMapperWrite<Dividend, DividendPostDto>, MapperDividend>();
        services.AddScoped<IMapperRead<Coefficient, CoefficientGetDto>, MapperCoefficient>();
        services.AddScoped<IMapperWrite<Coefficient, CoefficientPostDto>, MapperCoefficient>();

        services.AddScoped<IRestQueryService<Coefficient>, RestQueryQuarterService<Coefficient>>();
        services.AddScoped<IRestQueryService<Dividend>, RestQueryDateService<Dividend>>();
        services.AddScoped<IRestQueryService<Report>,RestQueryQuarterService<Report>>();
        services.AddScoped<IRestQueryService<Price>, RestQueryDateService<Price>>();
        services.AddScoped<IRestQueryService<Float>, RestQueryDateService<Float>>();
        services.AddScoped<IRestQueryService<Split>, RestQueryDateService<Split>>();

        services.AddScoped<CompanyApi>();
        services.AddScoped<CompanySourceApi>();
        services.AddScoped<RatingApi>();
        services.AddScoped<RestApiRead<Report, ReportGetDto>>();
        services.AddScoped<RestApiWrite<Report, ReportPostDto>>();
        services.AddScoped<RestApiRead<Price, PriceGetDto>>();
        services.AddScoped<RestApiWrite<Price, PricePostDto>>();
        services.AddScoped<RestApiRead<Float, FloatGetDto>>();
        services.AddScoped<RestApiWrite<Float, FloatPostDto>>();
        services.AddScoped<RestApiRead<Split, SplitGetDto>>();
        services.AddScoped<RestApiWrite<Split, SplitPostDto>>();
        services.AddScoped<RestApiRead<Dividend, DividendGetDto>>();
        services.AddScoped<RestApiWrite<Dividend, DividendPostDto>>();
        services.AddScoped<RestApiRead<Coefficient, CoefficientGetDto>>();
        services.AddScoped<RestApiWrite<Coefficient, CoefficientPostDto>>();

        services.AddTransient<CoefficientService>();
        services.AddTransient<DividendService>();
        services.AddTransient<FloatService>();
        services.AddTransient<PriceService>();
        services.AddTransient<RatingService>();
        services.AddTransient<ReportService>();
        services.AddTransient<SplitService>();

        services.AddScoped(typeof(DataLoader<>));
        services.AddTransient<IDataLoaderConfiguration<Price>,LoadPriceConfiguration>();
        services.AddTransient<IDataLoaderConfiguration<Report>,LoadReportConfiguration>();
        services.AddTransient<IDataLoaderConfiguration<Float>,LoadFloatConfiguration>();
        services.AddTransient<IDataLoaderConfiguration<Split>,LoadSplitConfiguration>();
        services.AddTransient<IDataLoaderConfiguration<Dividend>,LoadDividendConfiguration>();

        services.AddTransient<PriceProcess>();
        services.AddTransient<ReportProcess>();
        services.AddTransient<DividendProcess>();
        services.AddTransient<SplitProcess>();
        services.AddTransient<FloatProcess>();
        services.AddTransient<CoefficientProcess>();

        services.AddSingleton<RabbitAction>();
        services.AddHostedService<RabbitBackgroundService>();
        services.AddHostedService<ComputeRatingBackgroundService>();
        services.AddHostedService<LoadPriceBackgroundService>();
        services.AddHostedService<LoadReportBackgroundService>();
        services.AddHostedService<LoadFloatBackgroundService>();
        services.AddHostedService<LoadDividendBackgroundService>();
        services.AddHostedService<LoadSplitBackgroundService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment()) 
            app.UseDeveloperExceptionPage();

        app.UseRouting();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}