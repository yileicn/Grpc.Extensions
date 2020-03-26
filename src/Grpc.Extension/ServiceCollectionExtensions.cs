using Microsoft.Extensions.DependencyInjection;
using System;
using Grpc.Extension.Interceptors;
using Grpc.Extension.Internal;
using Microsoft.Extensions.Logging;
using OpenTracing;
using Microsoft.Extensions.Configuration;
using Grpc.Extension.Client;
using Grpc.Extension.Abstract;

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
        /// <typeparam name="TStartup">实现IGrpcService的类所在程序集下的任意类</typeparam>
        /// <param name="services"></param>
        /// <param name="conf"></param>
        /// <returns></returns>
        public static IServiceCollection AddGrpcExtensions<TStartup>(this IServiceCollection services, IConfiguration conf)
        {
            //添加IGrpService
            services.Scan(scan => scan
                .FromAssemblyOf<TStartup>()
                    .AddClasses(classes => classes.AssignableTo<IGrpcService>())
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime());
            //添加ServerBuilder
            services.AddSingleton<ServerBuilder>();
            //添加服务端中间件
            services.AddServerInterceptor<MonitorInterceptor>();
            services.AddServerInterceptor<ThrottleInterceptor>();
            //Jaeger
            services.AddJaeger(conf);

            //添加GrpcClient扩展
            services.AddGrpcClientExtensions(conf);

            return services;
        }

        
        /// <summary>
        /// 添加Jaeger和Interceptor
        /// </summary>
        /// <param name="services"></param>
        /// <param name="conf"></param>
        /// <returns></returns>
        private static IServiceCollection AddJaeger(this IServiceCollection services, IConfiguration conf)
        {
            var key = "GrpcServer:Jaeger";
            var jaegerOptions = conf.GetSection(key).Get<JaegerOptions>();
            if (jaegerOptions == null || jaegerOptions.Enable == false)
                return services;

            //jaeger
            services.AddSingleton<ITracer>(sp => {
                var options = GrpcServerOptions.Instance.Jaeger;
                var serviceName = options.ServiceName;
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
