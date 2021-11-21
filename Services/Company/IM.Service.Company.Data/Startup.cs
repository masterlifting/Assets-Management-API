using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Data.Clients.Price;
using IM.Service.Company.Data.Clients.Report;
using IM.Service.Company.Data.DataAccess;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Services.BackgroundServices;
using IM.Service.Company.Data.Services.DataServices.Prices;
using IM.Service.Company.Data.Services.DataServices.Reports;
using IM.Service.Company.Data.Services.DtoServices;
using IM.Service.Company.Data.Services.MqServices;
using IM.Service.Company.Data.Settings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Polly;

using System;

namespace IM.Service.Company.Data
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));

            services.AddDbContext<DatabaseContext>(provider =>
            {
                provider.UseLazyLoadingProxies();
                provider.UseNpgsql(Configuration["ServiceSettings:ConnectionStrings:Db"]);
            });

            services.AddControllers();

            services.AddHttpClient<TdAmeritradeClient>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));
            services.AddHttpClient<MoexClient>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));
            services.AddHttpClient<InvestingClient>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));

            services.AddScoped<PriceParser>();
            services.AddScoped<ReportParser>();

            services.AddScoped<PriceLoader>();
            services.AddScoped<ReportLoader>();

            services.AddScoped<PricesDtoManager>();
            services.AddScoped<ReportsDtoManager>();
            services.AddScoped<StockSplitsDtoManager>();
            services.AddScoped<StockVolumesDtoManager>();

            services.AddScoped(typeof(RepositorySet<>));
            services.AddScoped<IRepositoryHandler<DataAccess.Entities.Company>, CompanyRepository>();
            services.AddScoped<IRepositoryHandler<Price>, PriceRepository>();
            services.AddScoped<IRepositoryHandler<Report>, ReportRepository>();
            services.AddScoped<IRepositoryHandler<StockSplit>, StockSplitRepository>();
            services.AddScoped<IRepositoryHandler<StockVolume>, StockVolumeRepository>();
            services.AddScoped<IRepositoryHandler<CompanySourceType>, CompanySourceTypeRepository>();

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
