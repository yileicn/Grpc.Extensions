using Consul;

namespace Grpc.Extension.Discovery.Consul.Checks
{
    public interface IConsulCheck
    {
        AgentCheckRegistration GetCheck(AgentServiceRegistration registration);
    }
}