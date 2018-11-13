using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace FM.GrpcDashboard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(new ConfigurationBuilder().AddEnvironmentVariables().AddCommandLine(args).Build())
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    builder.SetBasePath(Path.Combine(AppContext.BaseDirectory))
                       .AddJsonFile("appsettings.json", false, true)
                       .AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", false, true);
                })
                .ConfigureLogging((ctx, builder) =>
                {
                    builder.AddFile(ctx.Configuration.GetSection("Logging"));
                })
                .UseStartup<Startup>()
                .Build();
    }
}
