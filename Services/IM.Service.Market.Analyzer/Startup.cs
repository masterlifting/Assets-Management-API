using IM.Service.Common.Net.HttpServices.JsonConvertors;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Analyzer.Clients;
using IM.Service.Market.Analyzer.DataAccess;
using IM.Service.Market.Analyzer.DataAccess.Entities;
using IM.Service.Market.Analyzer.DataAccess.Repositories;
using IM.Service.Market.Analyzer.Services.BackgroundServices;
using IM.Service.Market.Analyzer.Services.CalculatorServices;
using IM.Service.Market.Analyzer.Services.DtoServices;
using IM.Service.Market.Analyzer.Services.MqServices;
using IM.Service.Market.Analyzer.Settings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IM.Service.Market.Analyzer;

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

        services.AddControllers().AddJsonOptions(x =>
        {
            x.JsonSerializerOptions.Converters.Add(new TimeOnlyConverter());
            x.JsonSerializerOptions.Converters.Add(new DateOnlyConverter());
        });

        services.AddHttpClient<CompanyDataClient>();

        services.AddSingleton<AnalyzerService>();
        services.AddScoped<RatingService>();
        services.AddScoped<CalculatorData>();

        services.AddScoped<RatingDtoManager>();

        services.AddScoped(typeof(Repository<>));
        services.AddScoped<IRepositoryHandler<Company>, CompanyRepository>();
        services.AddScoped<IRepositoryHandler<AnalyzedEntity>, AnalyzedEntityRepository>();
        services.AddScoped<IRepositoryHandler<Rating>, RatingRepository>();

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