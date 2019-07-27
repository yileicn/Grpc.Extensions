using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grpc.Extension.LoadBalancer
{
    /// <summary>
    /// 随机负载
    /// </summary>
    public class RandomLoadBalancer : ILoadBalancer
    {
        /// <summary>
        /// 随机获取Endpoint
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="endpoints"></param>
        /// <returns></returns>
        public string SelectEndpoint(string serviceName, List<string> endpoints)
        {
            endpoints = endpoints.OrderBy(q => Guid.NewGuid()).ToList();
            return endpoints.FirstOrDefault();
        }
    }
}
