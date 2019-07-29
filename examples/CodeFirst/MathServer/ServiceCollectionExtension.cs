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
    }
}
