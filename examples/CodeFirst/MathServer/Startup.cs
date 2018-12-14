using Grpc.Extension;
using Grpc.Extension.BaseService;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathServer
{
    public class Startup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            //grpc
            services.Scan(scan => scan
                .FromAssemblyOf<Startup>()
                    .AddClasses(classes => classes.AssignableTo<IGrpcService>())
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime());
            services.AddGrpcExtensions(); //注入GrpcExtensions
            services.AddHostedService<GrpcHostServiceV2>();

            return services.BuildServiceProvider();
        }
    }
}
