using Grpc.Extension;
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
        }
    }
}
