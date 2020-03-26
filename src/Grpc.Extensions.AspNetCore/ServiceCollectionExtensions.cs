using Grpc.AspNetCore.Server;
using Grpc.AspNetCore.Server.Model;
using Grpc.Extension;
using Grpc.Extension.AspNetCore;
using Grpc.Extension.Client;
using Grpc.Extension.Interceptors;
using Grpc.Extensions.AspNetCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTracing;
using System;

namespace Grpc.Extensions.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加Grpc扩展
        /// </summary>
        /// <param name="services"></param>
        /// <param name="conf"></param>
        public static void AddGrpcExtensions(this IServiceCollection services, IConfiguration conf) => AddGrpcExtensions(services, conf, null);

        /// <summary>
        /// 添加Grpc扩展
        /// </summary>
        /// <param name="services"></param>
        /// <param name="conf"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IGrpcServerBuilder AddGrpcExtensions(this IServiceCollection services, IConfiguration conf, Action<GrpcServiceOptions> configureOptions)
        {
            //注入配制
            services.Configure<GrpcServerOptions>(conf.GetSection("GrpcServer"));
            //AddGrpc
            var builder = services.AddGrpc(options => {
                options.Interceptors.Add<MonitorInterceptor>();
                options.Interceptors.Add<ThrottleInterceptor>();
                //Jaeger
                options.AddJaegerInterceptor(conf);
                //执行配制
                configureOptions?.Invoke(options);
            });
            //ServiceMethodProvider
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IServiceMethodProvider<>), typeof(BinderServiceMethodProvider<>)));
            //添加Jaeger
            services.AddJaeger(conf);
            //添加GrpcClient扩展
            services.AddGrpcClientExtensions(conf);
            //注册到服务发现
            services.AddHostedService<RegisterServiceHosted>();
            return builder;
        }

        /// <summary>
        /// 添加Jaeger
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
                var options = sp.GetService<IOptions<GrpcServerOptions>>().Value.Jaeger;
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

            return services;
        }

        /// <summary>
        /// 添加jaeger中间件 
        /// </summary>
        /// <param name="grpcServiceOptions"></param>
        /// <param name="conf"></param>
        private static void AddJaegerInterceptor(this GrpcServiceOptions grpcServiceOptions, IConfiguration conf)
        {
            var key = "GrpcServer:Jaeger";
            var jaegerOptions = conf.GetSection(key).Get<JaegerOptions>();
            if (jaegerOptions == null || jaegerOptions.Enable == false)
                return;

            //添加jaeger中间件
            grpcServiceOptions.Interceptors.Add<JaegerTracingInterceptor>();
        }
    }
}
