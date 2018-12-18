using Grpc.Extension;
using Grpc.Extension.BaseService;
using Grpc.Extension.Interceptors;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using Math;
using MathServer.Middlewares;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathServer
{
    public static class ServiceCollectionExtension
    {
        public static void AddGrpc(this IServiceCollection services)
        {
            //grpc
            services.Scan(scan => scan
                .FromAssemblyOf<MathGrpc>()
                    .AddClasses(classes => classes.AssignableTo<IGrpcService>())
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime());
            services.AddGrpcExtensions(); //注入GrpcExtensions
            services.AddHostedService<GrpcHostServiceV2>();
        }

        public static void AddJaeger(this IServiceCollection services,IConfiguration conf)
        {
            var provider = services.BuildServiceProvider();
            var loggerFactory = provider.GetService<ILoggerFactory>();
            //jaeger
            var tracer = new Tracer.Builder("MathServer")
                .WithLoggerFactory(loggerFactory)
                .WithSampler(new ConstSampler(true))
                .WithReporter(new RemoteReporter.Builder()
                    .WithFlushInterval(TimeSpan.FromSeconds(5))
                    .WithMaxQueueSize(5)
                    .WithSender(new UdpSender(conf["Jaeger:AgentIp"], conf.GetValue<int>("Jaeger:AgentPort"), 1024 * 5)).Build())
                .Build();
            GlobalTracer.Register(tracer);
            services.AddSingleton<ITracer>(tracer);
            //添加jaeger中间件
            services.AddSingleton<ServerInterceptor,JaegerTracingMiddleware>();
            services.AddSingleton<ClientInterceptor, ClientJaegerTracingMiddleware>();
        }
    }
}
