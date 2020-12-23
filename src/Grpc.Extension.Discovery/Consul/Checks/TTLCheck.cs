using Consul;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Grpc.Extension.Discovery.Consul.Checks
{
    public class TTLCheck : IConsulCheck
    {
        public AgentCheckRegistration GetCheck(AgentServiceRegistration registration)
        {
            var check = new NewAgentCheckRegistration
            {
                CheckID = $"service:{registration.ID}",//consul 1.3使用ID,consul 1.7使用CheckID
                Name = $"ttlcheck",
                TTL = TimeSpan.FromSeconds(15),
                Status = HealthStatus.Passing,
                DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
            };
            return check;
        }
    }
}
