using CommonServices.RabbitServices;

using IM.Services.Companies.Reports.Api.Clients;
using IM.Services.Companies.Reports.Api.DataAccess;
using IM.Services.Companies.Reports.Api.DataAccess.Entities;
using IM.Services.Companies.Reports.Api.DataAccess.Repository;
using IM.Services.Companies.Reports.Api.Services.BackgroundServices;
using IM.Services.Companies.Reports.Api.Services.DtoServices;
using IM.Services.Companies.Reports.Api.Services.RabbitServices;
using IM.Services.Companies.Reports.Api.Services.ReportServices;
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
        private static readonly QueueExchanges[] targetExchanges = new[] { QueueExchanges.crud, QueueExchanges.loader };
        private static readonly QueueNames[] targetQueues = new[] { QueueNames.companiesreportscrud, QueueNames.companiesreportsloader };
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));

            services.AddDbContext<ReportsContext>(provider =>
           {
               provider.UseLazyLoadingProxies();
               provider.UseNpgsql(Configuration["ServiceSettings:ConnectionStrings:Db"]);
           });

            services.AddControllers();

            services.AddHttpClient<InvestingClient>();

            services.AddSingleton<ReportParser>();
            services.AddScoped<ReportLoader>();
            services.AddScoped<ReportsDtoAgregator>();

            services.AddScoped(typeof(EntityRepository<>));
            services.AddScoped<IEntityChecker<Ticker>, TckerChecker>();
            services.AddScoped<IEntityChecker<ReportSource>, ReportSourceChecker>();

            services.AddSingleton(x => new RabbitBuilder(
                Configuration["ServiceSettings:ConnectionStrings:Mq"]
                ,QueueConfiguration.GetConfiguredData(targetExchanges, targetQueues)));
            services.AddSingleton<RabbitService>();
            services.AddSingleton<RabbitActionService>();
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
