using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grpc.Core;
using Grpc.Extension.Consul;
using Grpc.Extension.Model;
using System.Reflection;
using Grpc.Extension.Common;
using Grpc.Extension.Interceptors;
using Grpc.Extension.LoadBalancer;
using Grpc.Extension.Internal;
using Grpc.Extension.Discovery;

namespace Grpc.Extension
{
    /// <summary>
    /// ServiceCollectionExtensions
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加Grpc扩展
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddGrpcExtensions(this IServiceCollection services)
        {
            //添加服务端中间件
            services.AddSingleton<ServerInterceptor, MonitorInterceptor>();
            services.AddSingleton<ServerInterceptor, ThrottleInterceptor>();
            //添加客户端中间件的CallInvoker
            services.AddSingleton<AutoChannelCallInvoker>();
            services.AddSingleton<CallInvoker, InterceptorCallInvoker>();
            //默认使用consul服务注册,服务发现，在外面可以注入其它策略
            if (!services.Any(p => p.ServiceType == typeof(IServiceRegister)))
            {
                services.AddConsulDiscovery();
            }            
            //添加Channel的Manager
            services.AddSingleton<ChannelManager>();
            services.AddSingleton<GrpcClientManager>();
            //默认使用轮询负载策略，在外面可以注入其它策略
            if (!services.Any(p => p.ServiceType == typeof(ILoadBalancer)))
            {
                services.AddSingleton<ILoadBalancer, RoundLoadBalancer>();
            }
            GrpcExtensions.ServiceProvider = services.BuildServiceProvider();
            return services;
        }

        /// <summary>
        /// 添加GrpcClient,生成元数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="discoveryUrl"></param>
        /// <param name="discoveryServiceName"></param>
        /// <returns></returns>
        public static IServiceCollection AddGrpcClient<T>(this IServiceCollection services, string discoveryUrl,string discoveryServiceName) where T: ClientBase<T>
        {
            services.AddSingleton<T>();
            var channelConfig = new ChannelConfig
            {
                DiscoveryUrl = discoveryUrl,
                DiscoveryServiceName = discoveryServiceName
            };
            var bindFlags = BindingFlags.Static | BindingFlags.NonPublic;
            channelConfig.GrpcServiceName = typeof(T).DeclaringType.GetFieldValue<string>("__ServiceName", bindFlags);
            var channelManager = GrpcExtensions.ServiceProvider.GetService<ChannelManager>();
            channelManager.Configs.Add(channelConfig);
            return services;
        }
    }
}
