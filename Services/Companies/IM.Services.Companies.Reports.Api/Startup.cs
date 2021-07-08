using IM.Services.Companies.Reports.Api.Clients;
using IM.Services.Companies.Reports.Api.DataAccess;
using IM.Services.Companies.Reports.Api.Services.Agregators.Implementations;
using IM.Services.Companies.Reports.Api.Services.Agregators.Interfaces;
using IM.Services.Companies.Reports.Api.Services.Background;
using IM.Services.Companies.Reports.Api.Services.Background.Implementations;
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
               provider.UseNpgsql(Configuration["ConnectionString"]);
           });

            services.AddControllers();

            services.AddHttpClient<InvestingClient>();

            services.AddScoped<ILastReportDtoAgregator, LastReportDtoAgregator>();
            services.AddScoped<IHistoryReportDtoAgregator, HistoryReportDtoAgregator>();
            services.AddScoped<IReportUpdater, ReportUpdater>();
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
