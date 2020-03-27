using Grpc.Core;
using Grpc.Extension.Abstract;
using Grpc.Extension.Abstract.Discovery;
using Grpc.Extension.Client.Interceptors;
using Grpc.Extension.Client.Internal;
using Grpc.Extension.Client.LoadBalancer;
using Grpc.Extension.Client.Model;
using Grpc.Extension.Common;
using Grpc.Extension.Discovery;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTracing;
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
        /// <param name="conf"></param>
        /// <returns></returns>
        public static IServiceCollection AddGrpcClientExtensions(this IServiceCollection services, IConfiguration conf)
        {
            //注入配制
            services.Configure<GrpcClientOptions>(conf.GetSection("GrpcClient"));
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
            //添加客户端中间件
            services.AddClientCallTimeout();
            services.AddClientMonitor();
            //Jaeger
            services.AddClientJaeger(conf);

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
        private static IServiceCollection AddClientMonitor(this IServiceCollection services)
        {
            services.AddClientInterceptor<ClientMonitorInterceptor>();

            return services;
        }

        /// <summary>
        /// 添加客户端超时Interceptor
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IServiceCollection AddClientCallTimeout(this IServiceCollection services)
        {
            services.AddSingleton<ClientInterceptor>(sp => 
            {
                var options = sp.GetService<IOptions<GrpcClientOptions>>().Value;
                return new ClientCallTimeout(options.GrpcCallTimeOut);
            });
            return services;
        }

        /// <summary>
        /// 添加Jaeger
        /// </summary>
        /// <param name="services"></param>
        /// <param name="conf"></param>
        /// <returns></returns>
        public static IServiceCollection AddClientJaeger(this IServiceCollection services, IConfiguration conf)
        {
            //读取Jaeger配制
            var key = conf["GrpcServer:ServiceAddress"] != null ? "GrpcServer" : "GrpcClient";
            var jaegerOptions = conf.GetSection($"{key}:Jaeger").Get<JaegerOptions>();
            if (jaegerOptions == null || jaegerOptions.Enable == false)
                return services;

            //jaeger
            if (!services.Any(p => p.ServiceType == typeof(ITracer)))
            {
                services.AddSingleton<ITracer>(sp =>
                {
                    var options = sp.GetService<IOptions<GrpcClientOptions>>().Value;
                    var tracer = new Jaeger.Tracer.Builder(options.Jaeger.ServiceName)
                    .WithLoggerFactory(sp.GetService<ILoggerFactory>())
                    .WithSampler(new Jaeger.Samplers.ConstSampler(true))
                    .WithReporter(new Jaeger.Reporters.RemoteReporter.Builder()
                        .WithFlushInterval(TimeSpan.FromSeconds(5))
                        .WithMaxQueueSize(5)
                        .WithSender(new Jaeger.Senders.UdpSender(jaegerOptions.AgentIp, jaegerOptions.AgentPort, 1024 * 5)).Build())
                    .Build();
                    return tracer;
                });
            }                
            //添加jaeger中间件
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
