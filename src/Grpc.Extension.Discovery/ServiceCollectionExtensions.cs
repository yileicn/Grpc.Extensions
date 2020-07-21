using Grpc.Extension.Abstract.Discovery;
using Grpc.Extension.Discovery.Consul;
using Grpc.Extension.Discovery.Consul.Checks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Grpc.Extension.Discovery
{
    /// <summary>
    /// 添加服务注册,服务发现
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加服务注册,服务发现
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddConsulDiscovery(this IServiceCollection services, Action<ConsulServiceBuilder> builder = null)
        {
            if (!services.Any(p => p.ImplementationType == typeof(ConsulServiceRegister)))
            {
                services.AddSingleton<IServiceRegister, ConsulServiceRegister>();
                services.AddSingleton<IServiceDiscovery, ConsulServiceDiscovery>();
                services.AddSingleton<IConsulCheck, TTLCheck>();
            }
            var obj = new ConsulServiceBuilder(services);
            builder?.Invoke(obj);
            
            return services;
        }
    }
}
