using Grpc.Extension;
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
            services.AddGrpc();
            //jaeger
            services.AddJaeger(_conf);
        }
    }
}
