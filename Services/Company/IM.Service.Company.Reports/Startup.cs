
using CommonServices.RepositoryService;

using IM.Service.Company.Reports.Clients;
using IM.Service.Company.Reports.DataAccess;
using IM.Service.Company.Reports.DataAccess.Entities;
using IM.Service.Company.Reports.DataAccess.Repository;
using IM.Service.Company.Reports.Services.BackgroundServices;
using IM.Service.Company.Reports.Services.DtoServices;
using IM.Service.Company.Reports.Services.RabbitServices;
using IM.Service.Company.Reports.Services.ReportServices;
using IM.Service.Company.Reports.Settings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Polly;

using System;

namespace IM.Service.Company.Reports
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

            services.AddHttpClient<InvestingClient>()
                .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
                .AddTransientHttpErrorPolicy(policy => policy.CircuitBreakerAsync(3, TimeSpan.FromSeconds(30)));

            services.AddScoped<ReportParser>();
            services.AddScoped<ReportLoader>();
            services.AddScoped<DtoManager>();

            services.AddScoped<IRepository<Ticker>, TickerRepository>();
            services.AddScoped<IRepository<Report>, ReportRepository>();
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
