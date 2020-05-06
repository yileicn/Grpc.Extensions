using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.Common
{
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// UseStartup
        /// </summary>
        /// <typeparam name="TStartup"></typeparam>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IHostBuilder UseStartup<TStartup>(this IHostBuilder hostBuilder) where TStartup : class
        {
            hostBuilder.ConfigureServices((ctx, services) => {
                //build
                services.AddSingleton<TStartup>();
                var provider = services.BuildServiceProvider();
                //get service
                dynamic startup = provider.GetService<TStartup>();
                //dynamic invoke
                startup.ConfigureServices(services);
            });
            return hostBuilder;
        }
    }
}
