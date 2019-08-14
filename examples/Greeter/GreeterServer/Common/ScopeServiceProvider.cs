using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace GreeterServer.Common
{
    public static class ScopeServiceProvider
    {
        private static readonly AsyncLocal<IServiceProvider> _current = new AsyncLocal<IServiceProvider>();

        public static IServiceProvider Current
        {
            get { return _current.Value; }
            set { _current.Value = value; }
        }

        public static T GetService<T>()
        {
            return Current.GetService<T>();
        }

        public static List<T> GetServices<T>()
        {
            return Current.GetServices<T>()?.ToList();
        }
    }
}
