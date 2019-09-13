using Grpc.Extension.Abstract.Discovery;
using Grpc.Extension.Discovery.Consul;
using Microsoft.Extensions.DependencyInjection;

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
        public static IServiceCollection AddConsulDiscovery(this IServiceCollection services)
        {
            services.AddSingleton<IServiceRegister, ConsulServiceRegister>();
            services.AddSingleton<IServiceDiscovery, ConsulServiceDiscovery>();

            return services;
        }
    }
}
