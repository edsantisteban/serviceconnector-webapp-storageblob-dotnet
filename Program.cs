using DotNetEnv;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
 
namespace WebStorageSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Env.Load(".env");
 
            CreateHostBuilder(args).Build().Run();
        }
 
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}