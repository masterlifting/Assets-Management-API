using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace IM.Service.Company.Prices
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                // .ConfigureLogging((context, logging) =>
                // {
                //     if(context.HostingEnvironment.IsProduction())
                //     {
                //         logging.ClearProviders();
                //         logging.AddJsonConsole();
                //     }
                // })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}