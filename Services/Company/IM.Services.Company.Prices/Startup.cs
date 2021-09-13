using CommonServices.RepositoryService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Polly;

using System;
using IM.Services.Company.Prices.DataAccess;
using IM.Services.Company.Prices.DataAccess.Entities;
using IM.Services.Company.Prices.DataAccess.Repository;
using IM.Services.Company.Prices.Services.BackgroundServices;
using IM.Services.Company.Prices.Services.DtoServices;
using IM.Services.Company.Prices.Services.HealthCheck;
using IM.Services.Company.Prices.Services.MapServices;
using IM.Services.Company.Prices.Services.PriceServices;
using IM.Services.Company.Prices.Services.RabbitServices;
using IM.Services.Company.Prices.Settings;

namespace IM.Services.Company.Prices
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));

            services.AddDbContext<PricesContext>(provider =>
            {
                provider.UseLazyLoadingProxies();
                provider.UseNpgsql(Configuration["ServiceSettings:ConnectionStrings:Db"]);
            });

            services.AddControllers();

            services.AddHttpClient<TdAmeritradeClient>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(10, retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp))))
                .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(10)));
            services.AddHttpClient<MoexClient>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(10, retryAttemp => TimeSpan.FromSeconds(Math.Pow(2, retryAttemp))))
                .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(10)));

            services.AddHealthChecks()
                .AddCheck<TdAmeritradeHealthCheck>("TdAmeritrade")
                .AddCheck<MoexHealthCheck>("Moex");

            services.AddTransient<PriceMapper>();
            services.AddScoped<PriceParser>();
            services.AddScoped<PriceLoader>();
            services.AddScoped<PriceDtoAggregator>();

            services.AddScoped<IRepository<Ticker>, TickerRepository>();
            services.AddScoped<IRepository<Price>, PriceRepository>();
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
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
