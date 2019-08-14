using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace GreeterServer.Common
{
    /// <summary>
    /// container manager
    /// </summary>
    public static class AppServiceProvider
    {
        public static IServiceProvider ServiceProvider { get; set; }

        public static T GetService<T>()
        {
            return ServiceProvider.GetService<T>();
        }

        public static List<T> GetServices<T>()
        {
            return ServiceProvider.GetServices<T>()?.ToList();
        }
    }
}
