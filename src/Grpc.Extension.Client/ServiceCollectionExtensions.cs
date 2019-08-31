using Grpc.Core;
using Grpc.Extension.Abstract;
using Grpc.Extension.Abstract.Discovery;
using Grpc.Extension.Client.Interceptors;
using Grpc.Extension.Client.Internal;
using Grpc.Extension.Client.LoadBalancer;
using Grpc.Extension.Client.Model;
using Grpc.Extension.Common;
using Grpc.Extension.Discovery;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Grpc.Extension.Client
{
    /// <summary>
    /// ServiceCollectionExtensions
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加GrpcClient扩展
        /// </summary>
        /// <param name="services"></param>
        /// <param name="useLogger"></param>
        /// <returns></returns>
        public static IServiceCollection AddGrpcClientExtensions(this IServiceCollection services, Action<LoggerAccessor> useLogger = null)
        {
            //GrpcClientApp
            services.AddSingleton<GrpcClientApp>();
            //添加客户端中间件的CallInvoker
            services.AddSingleton<AutoChannelCallInvoker>();
            services.AddSingleton<CallInvoker, InterceptorCallInvoker>();
            //添加Channel的Manager
            services.AddSingleton<ChannelPool>();
            services.AddSingleton<GrpcClientManager>();

            //默认使用轮询负载策略，在外面可以注入其它策略
            if (!services.Any(p => p.ServiceType == typeof(ILoadBalancer)))
            {
                services.AddSingleton<ILoadBalancer, RoundLoadBalancer>();
            }

            //默认使用consul服务注册,服务发现，在外面可以注入其它策略
            if (!services.Any(p => p.ServiceType == typeof(IServiceRegister)))
            {
                services.AddConsulDiscovery();
            }

            //添加缓存
            services.AddMemoryCache();

            //配制日志
            if (useLogger != null)
            {
                //添加客户端日志监控
                services.AddClientMonitor();
                useLogger?.Invoke(LoggerAccessor.Instance);
            }

            return services;
        }

        /// <summary>
        /// 添加GrpcClient,生成元数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="discoveryServiceName">Discovery上客户端服务名字</param>
        /// <param name="discoveryUrl">Discovery的服务器地址</param>
        /// <param name="channelOptions">ChannelOption</param>
        /// <returns></returns>
        public static IServiceCollection AddGrpcClient<T>(this IServiceCollection services, string discoveryServiceName, string discoveryUrl = "", IEnumerable<ChannelOption> channelOptions = null) where T : ClientBase<T>
        {
            services.AddSingleton<T>();
            var channelConfig = new ChannelConfig
            {
                DiscoveryUrl = discoveryUrl,
                DiscoveryServiceName = discoveryServiceName,
                ChannelOptions = channelOptions
            };
            var bindFlags = BindingFlags.Static | BindingFlags.NonPublic;
            channelConfig.GrpcServiceName = typeof(T).DeclaringType.GetFieldValue<string>("__ServiceName", bindFlags);
            ChannelPool.Configs.Add(channelConfig);
            return services;
        }

        /// <summary>
        /// 添加客户端日志监控Interceptor
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddClientMonitor(this IServiceCollection services)
        {
            services.AddClientInterceptor<ClientMonitorInterceptor>();

            return services;
        }

        /// <summary>
        /// 添加客户端超时Interceptor
        /// </summary>
        /// <param name="services"></param>
        /// <param name="callTimeOutSecond"></param>
        /// <returns></returns>
        public static IServiceCollection AddClientCallTimeout(this IServiceCollection services, double callTimeOutSecond)
        {
            services.AddSingleton<ClientInterceptor>(new ClientCallTimeout(callTimeOutSecond));

            return services;
        }

        /// <summary>
        /// 添加客户端Jaeger Interceptor
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddClientJaeger(this IServiceCollection services)
        {
            services.AddClientInterceptor<ClientJaegerTracingInterceptor>();

            return services;
        }

        /// <summary>
        /// 添加客户端Interceptor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddClientInterceptor<T>(this IServiceCollection services) where T : ClientInterceptor
        {
            services.AddSingleton<ClientInterceptor, T>();

            return services;
        }
    }
}
