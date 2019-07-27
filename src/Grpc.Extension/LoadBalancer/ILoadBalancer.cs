using System.Collections.Generic;

namespace Grpc.Extension.LoadBalancer
{
    /// <summary>
    /// 负载均衡
    /// </summary>
    public interface ILoadBalancer
    {
        /// <summary>
        /// 选择Endpoint
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="endpoints"></param>
        /// <returns></returns>
        string SelectEndpoint(string serviceName, List<string> endpoints);
    }
}