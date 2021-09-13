
using CommonServices.RepositoryService;
using IM.Services.Company.Reports.Clients;
using IM.Services.Company.Reports.DataAccess;
using IM.Services.Company.Reports.DataAccess.Entities;
using IM.Services.Company.Reports.DataAccess.Repository;
using IM.Services.Company.Reports.Services.BackgroundServices;
using IM.Services.Company.Reports.Services.DtoServices;
using IM.Services.Company.Reports.Services.RabbitServices;
using IM.Services.Company.Reports.Services.ReportServices;
using IM.Services.Company.Reports.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IM.Services.Company.Reports
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
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

            services.AddScoped<ReportParser>();
            services.AddScoped<ReportLoader>();
            services.AddScoped<ReportsDtoAggregator>();

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
