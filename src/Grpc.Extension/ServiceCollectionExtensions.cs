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
using Microsoft.Extensions.Logging;
using OpenTracing;
using Microsoft.Extensions.Configuration;

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
            //添加ServerBuilder
            services.AddSingleton<ServerBuilder>();
            //添加服务端中间件
            services.AddServerInterceptor<MonitorInterceptor>();
            services.AddServerInterceptor<ThrottleInterceptor>();

            //添加GrpcClient扩展
            services.AddGrpcClientExtensions();

            return services;
        }

        /// <summary>
        /// 添加GrpcClient扩展
        /// </summary>
        /// <param name="services"></param>
        /// <param name="useLogger"></param>
        /// <returns></returns>
        public static IServiceCollection AddGrpcClientExtensions(this IServiceCollection services, Action<LoggerAccessor> useLogger = null)
        {
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
        /// <param name="discoveryUrl">Discovery的服务器地址</param>
        /// <param name="discoveryServiceName">Discovery上客户端服务名字</param>
        /// <param name="channelOptions">ChannelOption</param>
        /// <returns></returns>
        public static IServiceCollection AddGrpcClient<T>(this IServiceCollection services, string discoveryUrl, string discoveryServiceName, IEnumerable<ChannelOption> channelOptions = null) where T : ClientBase<T>
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
        /// 添加Jaeger和Interceptor
        /// </summary>
        /// <param name="services"></param>
        /// <param name="conf"></param>
        /// <returns></returns>
        public static IServiceCollection AddJaeger(this IServiceCollection services, IConfiguration conf)
        {
            var key = "GrpcServer:Jaeger";
            var jaegerOptions = conf.GetSection(key).Get<JaegerOptions>();
            if (jaegerOptions == null)
                throw new ArgumentException($"{key} Value cannot be null");

            if (string.IsNullOrWhiteSpace(jaegerOptions.AgentIp))
                throw new ArgumentException($"{key}:AgentIp Value cannot be null");

            if (jaegerOptions.AgentPort == 0)
                throw new ArgumentNullException($"{key}:AgentPort Value cannot be null");

            //jaeger
            services.AddSingleton<ITracer>(sp => {
                var serviceName = jaegerOptions.ServiceName ?? GrpcServerOptions.Instance.DiscoveryServiceName;
                var tracer = new Jaeger.Tracer.Builder(serviceName)
               .WithLoggerFactory(sp.GetService<ILoggerFactory>())
               .WithSampler(new Jaeger.Samplers.ConstSampler(true))
               .WithReporter(new Jaeger.Reporters.RemoteReporter.Builder()
                   .WithFlushInterval(TimeSpan.FromSeconds(5))
                   .WithMaxQueueSize(5)
                   .WithSender(new Jaeger.Senders.UdpSender(jaegerOptions.AgentIp, jaegerOptions.AgentPort, 1024 * 5)).Build())
               .Build();
                return tracer;
            });

            //添加jaeger中间件
            services.AddServerInterceptor<JaegerTracingInterceptor>();
            services.AddClientInterceptor<ClientJaegerTracingInterceptor>();

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
        /// 添加服务端Interceptor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddServerInterceptor<T>(this IServiceCollection services) where T : ServerInterceptor
        {
            services.AddSingleton<ServerInterceptor, T>();

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
