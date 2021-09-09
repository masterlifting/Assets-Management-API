using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.Clients;
using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.DataAccess.Repository;
using IM.Services.Analyzer.Api.Services.BackgroundServices;
using IM.Services.Analyzer.Api.Services.CalculatorServices;
using IM.Services.Analyzer.Api.Services.DtoServices;
using IM.Services.Analyzer.Api.Services.RabbitServices;
using IM.Services.Analyzer.Api.Settings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IM.Services.Analyzer.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));

            services.AddDbContext<AnalyzerContext>(provider =>
            {
                provider.UseLazyLoadingProxies();
                provider.UseNpgsql(Configuration["ServiceSettings:ConnectionStrings:Db"]);
                provider.EnableSensitiveDataLogging();
            });

            services.AddControllers();

            services.AddHttpClient<PricesClient>();
            services.AddHttpClient<ReportsClient>();

            services.AddScoped<CoefficientDtoAggregator>();
            services.AddScoped<RatingDtoAggregator>();
            services.AddScoped<RecommendationDtoAggregator>();

            services.AddScoped<IRepository<Ticker>, TickerRepository>();
            services.AddScoped<IRepository<Price>, PriceRepository>();
            services.AddScoped<IRepository<Report>, ReportRepository>();
            services.AddScoped<IRepository<Rating>, RatingRepository>();
            services.AddScoped(typeof(RepositorySet<>));

            services.AddScoped<ReportCalculator>();
            services.AddScoped<PriceCalculator>();
            services.AddScoped<RatingCalculator>();

            services.AddScoped<RabbitActionService>();
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
}
