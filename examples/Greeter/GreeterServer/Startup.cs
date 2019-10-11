using GreeterServer.Middlewares;
using Grpc.Extension;
using Grpc.Extension.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GreeterServer
{
    public class Startup
    {
        private readonly IConfiguration _conf;

        public Startup(IConfiguration conf)
        {
            this._conf = conf;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpcExtensions(); //注入GrpcExtensions
            services.AddServerInterceptor<RequestServicesMiddleware>();
            services.AddClientMonitor();
            services.AddGrpcClient<MathGrpc.MathGrpc.MathGrpcClient>("Math.Test");
            services.AddHostedService<GrpcHostServiceV2>();
        }
    }
}
