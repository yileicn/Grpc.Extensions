using Grpc.Extension.Discovery;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.Consul
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
