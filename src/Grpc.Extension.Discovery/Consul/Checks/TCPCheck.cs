using Consul;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.Discovery.Consul.Checks
{
    public class TCPCheck : IConsulCheck
    {
        public AgentCheckRegistration GetCheck(AgentServiceRegistration registration)
        {
            var check = new AgentCheckRegistration
            {
                Name = "tcpcheck",
                TCP = $"{registration.Address}:{registration.Port}",
                Interval = TimeSpan.FromSeconds(15),
                Timeout = TimeSpan.FromSeconds(3),
                Status = HealthStatus.Passing,
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
            };
            return check;
        }
    }
}
