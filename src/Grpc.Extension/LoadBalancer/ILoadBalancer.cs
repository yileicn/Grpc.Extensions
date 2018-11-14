using System.Collections.Generic;

namespace Grpc.Extension.LoadBalancer
{
    public interface ILoadBalancer
    {
        string SelectEndpoint(string serviceName, List<string> endpoints);
    }
}