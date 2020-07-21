using Grpc.Extension.Discovery.Consul.Checks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.Discovery.Consul
{
    public class ConsulServiceBuilder
    {
        private readonly IServiceCollection services;

        public ConsulServiceBuilder(IServiceCollection services)
        {
            this.services = services;
        }

        public ConsulServiceBuilder UseTTLCheck()
        {
            services.AddSingleton<IConsulCheck, TTLCheck>();

            return this;
        }

        public ConsulServiceBuilder UseTCPCheck()
        {
            services.AddSingleton<IConsulCheck, TCPCheck>();

            return this;
        }
    }
}
