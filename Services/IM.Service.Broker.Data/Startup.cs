using IM.Service.Broker.Data.DataAccess;
using IM.Service.Broker.Data.DataAccess.Entities;
using IM.Service.Broker.Data.DataAccess.Repository;
using IM.Service.Broker.Data.Services.BackgroundServices;
using IM.Service.Broker.Data.Services.DataServices.Reports;
using IM.Service.Broker.Data.Services.DtoServices;
using IM.Service.Broker.Data.Services.MqServices;
using IM.Service.Broker.Data.Settings;
using IM.Service.Common.Net.RepositoryService;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IM.Service.Broker.Data;

public class Startup
{
    private IConfiguration Configuration { get; }
    public Startup(IConfiguration configuration) => Configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<ServiceSettings>(Configuration.GetSection(nameof(ServiceSettings)));

        services.AddMemoryCache();

        services.AddDbContext<DatabaseContext>(x =>
            x.UseNpgsql(Configuration["ServiceSettings:ConnectionStrings:Db"]));

        services.AddControllers();

        services.AddScoped(typeof(Repository<>));
        services.AddScoped<IRepositoryHandler<User>, UserRepository>();
        services.AddScoped<IRepositoryHandler<Account>, AccountRepository>();
        services.AddScoped<IRepositoryHandler<Company>, CompanyRepository>();
        services.AddScoped<IRepositoryHandler<DataAccess.Entities.Broker>, BrokerRepository>();
        services.AddScoped<IRepositoryHandler<TransactionAction>, TransactionActionRepository>();
        services.AddScoped<IRepositoryHandler<Transaction>, TransactionRepository>();
        services.AddScoped<IRepositoryHandler<Report>, ReportRepository>();
        services.AddScoped<IRepositoryHandler<Stock>, StockRepository>();

        services.AddScoped<ReportDtoManager>();
        services.AddScoped<BrokerDtoManager>();
        services.AddScoped<TransactionActionDtoManager>();

        services.AddScoped<ReportGrabber>();
        services.AddScoped<ReportLoader>();

        services.AddSingleton<RabbitActionService>();
        services.AddHostedService<RabbitBackgroundService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();

        app.UseRouting();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}