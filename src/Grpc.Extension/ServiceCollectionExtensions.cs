using Microsoft.Extensions.DependencyInjection;
using System;
using Grpc.Extension.Interceptors;
using Grpc.Extension.Internal;
using Microsoft.Extensions.Logging;
using OpenTracing;
using Microsoft.Extensions.Configuration;
using Grpc.Extension.Client;

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
            services.AddClientJaeger();

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
    }
}
