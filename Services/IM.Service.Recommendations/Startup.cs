
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Recommendations.Clients;
using IM.Service.Recommendations.DataAccess;
using IM.Service.Recommendations.DataAccess.Entities;
using IM.Service.Recommendations.DataAccess.Repository;
using IM.Service.Recommendations.Services.BackgroundServices;
using IM.Service.Recommendations.Services.DtoServices;
using IM.Service.Recommendations.Services.MqServices;
using IM.Service.Recommendations.Settings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            provider.EnableSensitiveDataLogging();
        });

        services.AddControllers();

        services.AddHttpClient<CompanyAnalyzerClient>();

        services.AddScoped<PurchaseDtoAggregator>();
        services.AddScoped<SaleDtoAggregator>();

        services.AddScoped<RepositoryHandler<Company>, CompanyRepository>();
        services.AddScoped(typeof(Repository<>));

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