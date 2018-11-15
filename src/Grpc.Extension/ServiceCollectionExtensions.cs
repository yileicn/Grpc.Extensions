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

namespace Grpc.Extension
{
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
            //添加Consul,Channel的Manager
            services.AddSingleton<ConsulManager>();
            services.AddSingleton<ChannelManager>();
            //默认使用轮询负载策略 后续可扩展其他策略（基于session, 随机等）
            if (! services.Any(p => p.ServiceType == typeof(ILoadBalancer)))
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
        /// <param name="consulUrl"></param>
        /// <param name="consulServiceName"></param>
        /// <returns></returns>
        public static IServiceCollection AddGrpcClient<T>(this IServiceCollection services, string consulUrl,string consulServiceName) where T: class
        {
            services.AddSingleton<T>();
            var channelConfig = new ChannelConfig
            {
                ConsulUrl = consulUrl,
                ConsulServiceName = consulServiceName
            };
            var bindFlags = BindingFlags.Static | BindingFlags.NonPublic;
            channelConfig.GrpcServiceName = typeof(T).DeclaringType.GetFieldValue<string>("__ServiceName", bindFlags);
            var channelManager = GrpcExtensions.ServiceProvider.GetService<ChannelManager>();
            channelManager.Configs.Add(channelConfig);
            return services;
        }
    }
}
