using IM.Service.Common.Net.HttpServices.JsonConvertors;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Data.Clients;
using IM.Service.Data.Domain.DataAccess;
using IM.Service.Data.Domain.DataAccess.Repositories;
using IM.Service.Data.Domain.Entities;
using IM.Service.Data.Domain.Entities.Catalogs;
using IM.Service.Data.Domain.Entities.ManyToMany;
using IM.Service.Data.Services.Background;
using IM.Service.Data.Services.DataFounders.Floats;
using IM.Service.Data.Services.DataFounders.Prices;
using IM.Service.Data.Services.DataFounders.Reports;
using IM.Service.Data.Services.DataFounders.Splits;
using IM.Service.Data.Services.Mq;
using IM.Service.Data.Services.RestApi;
using IM.Service.Data.Services.RestApi.Common;
using IM.Service.Data.Settings;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace IM.Service.Data;

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
        services.AddScoped<IMapperEdit<Report, ReportGetDto>>();
        services.AddScoped<MapperRepport>();

        services.AddScoped<IRestQueryService<Report>,RestQueryQuarterService<Report>>();


        services.AddScoped<PriceGrabber>();
        services.AddScoped<ReportGrabber>();
        services.AddScoped<StockSplitGrabber>();
        services.AddScoped<StockVolumeGrabber>();

        services.AddScoped<PriceLoader>();
        services.AddScoped<ReportLoader>();
        services.AddScoped<StockSplitLoader>();
        services.AddScoped<StockVolumeLoader>();

        services.AddScoped<CompanyApi>();
        services.AddScoped<PriceApi>();
        services.AddScoped<ReportApi>();
        services.AddScoped<SplitApi>();
        services.AddScoped<FloatApi>();

        services.AddScoped(typeof(Repository<>));
        services.AddScoped<IRepositoryHandler<Company>, CompanyRepository>();
        services.AddScoped<IRepositoryHandler<Industry>, IndustryRepository>();
        services.AddScoped<IRepositoryHandler<Sector>, SectorRepository>();
        services.AddScoped<IRepositoryHandler<Price>, PriceRepository>();
        services.AddScoped<IRepositoryHandler<Report>, ReportRepository>();
        services.AddScoped<IRepositoryHandler<Split>, SplitRepository>();
        services.AddScoped<IRepositoryHandler<Float>, FloatRepository>();
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