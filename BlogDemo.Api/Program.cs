using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BlogDemo.Infrastructure.Database;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace BlogDemo.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft",LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine("logs",@"log.txt"),rollingInterval:RollingInterval.Day)
                .CreateLogger();
            var host=CreateWebHostBuilder(args).Build();
            using (var scope=host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFoctory = services.GetRequiredService<ILoggerFactory>();
                try
                {
                    var myContext = services.GetRequiredService<MyContext>();
                    MyContextSeed.SeedAsync(myContext,loggerFoctory).Wait();
                }
                catch (Exception e)
                {
                    var logger = loggerFoctory.CreateLogger<Program>();
                    logger.LogError(e,"Error occured seeding the Database.");
                }
            }
            
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                //.UseStartup<Startup>();
                .UseStartup(typeof(StartupDevelopment).GetTypeInfo().Assembly.FullName)
                .UseSerilog();
    }
}
