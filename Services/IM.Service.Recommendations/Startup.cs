using CommonServices.RepositoryService;

using IM.Gateway.Recommendations.Clients;
using IM.Gateway.Recommendations.DataAccess;
using IM.Gateway.Recommendations.DataAccess.Entities;
using IM.Gateway.Recommendations.DataAccess.Repository;
using IM.Gateway.Recommendations.Services.BackgroundServices;
using IM.Gateway.Recommendations.Services.DtoServices;
using IM.Gateway.Recommendations.Services.RabbitServices;
using IM.Gateway.Recommendations.Settings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IM.Gateway.Recommendations
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));

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

            services.AddScoped<IRepositoryHandler<Ticker>, TickerRepository>();
            services.AddScoped(typeof(RepositorySet<>));

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
}
