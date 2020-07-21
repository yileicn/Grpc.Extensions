using Grpc.Extension;
using Grpc.Extension.Discovery;
using Math;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MathServer
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
            //grpc
            services.AddGrpcExtensions<MathGrpc>(_conf); //注入GrpcExtensions
            services.AddHostedService<GrpcHostServiceV2>();
            //使用TCPCheck,默认使用TTLCheck
            services.AddConsulDiscovery(p => p.UseTCPCheck());
        }
    }
}
