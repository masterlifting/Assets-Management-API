using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.Services.Agregators.Implementations;
using IM.Services.Analyzer.Api.Services.Agregators.Interfaces;
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
                provider.UseNpgsql(Configuration["ConnectionString"]);
            });

            services.AddControllers();

            services.AddScoped<ICoefficientDtoAgregator, CoefficientDtoAgregator>();
            services.AddScoped<IRatingDtoAgregator, RatingDtoAgregator>();
            services.AddScoped<IRecommendationDtoAgregator, RecommendationDtoAgregator>();
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
