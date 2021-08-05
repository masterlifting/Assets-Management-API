using IM.Services.Companies.Reports.Api.Clients;
using IM.Services.Companies.Reports.Api.DataAccess;
using IM.Services.Companies.Reports.Api.DataAccess.Entities;
using IM.Services.Companies.Reports.Api.DataAccess.Repository;
using IM.Services.Companies.Reports.Api.Services.Agregators.Implementations;
using IM.Services.Companies.Reports.Api.Services.Agregators.Interfaces;
using IM.Services.Companies.Reports.Api.Services.Background.RabbitMqBackgroundServices;
using IM.Services.Companies.Reports.Api.Services.Background.ReportUpdaterBackgroundServices.Implementations;
using IM.Services.Companies.Reports.Api.Services.Background.ReportUpdaterBackgroundServices.Interfaces;
using IM.Services.Companies.Reports.Api.Settings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IM.Services.Companies.Reports.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));

            services.AddDbContext<ReportsContext>(provider =>
           {
               provider.UseLazyLoadingProxies();
               //todo: connection string to improve
               provider.UseNpgsql(Configuration["ServiceSettings:ConnectionStrings:Db"]);
           });

            services.AddControllers();

            services.AddHttpClient<InvestingClient>();

            services.AddScoped<IReportsDtoAgregator, ReportsDtoAgregator>();
            services.AddScoped<IReportUpdater, ReportUpdater>();

            services.AddScoped<IEntityChecker<Ticker>, TckerChecker>();
            services.AddScoped<IEntityChecker<ReportSource>, ReportSourceChecker>();
            services.AddScoped(typeof(EntityRepository<,>));

            services.AddHostedService<RabbitmqBackgroundService>();
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
