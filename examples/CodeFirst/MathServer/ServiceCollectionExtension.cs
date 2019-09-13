using Grpc.Extension;
using Grpc.Extension.Abstract;
using Math;
using Microsoft.Extensions.DependencyInjection;


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
