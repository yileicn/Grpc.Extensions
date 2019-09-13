using Grpc.Extension.Abstract;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Grpc.Extension.Client.LoadBalancer
{
    /// <summary>
    /// 轮询负载
    /// </summary>
    public class RoundLoadBalancer : ILoadBalancer 
    {
        private ConcurrentDictionary<string, int> _serviceInvokeIndexs = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// 轮询获取Endpoint
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="endpoints"></param>
        /// <returns></returns>
        public string SelectEndpoint(string serviceName, List<string> endpoints)
        {
            endpoints = endpoints.OrderBy(q => q).ToList();
            var index = _serviceInvokeIndexs.GetOrAdd(serviceName, 0);
            if (index >= endpoints.Count)
            {
                index = _serviceInvokeIndexs.AddOrUpdate(serviceName, 0, (k, v) => 0);
            }
            _serviceInvokeIndexs.AddOrUpdate(serviceName, index, (k, v) => v + 1);
            return endpoints.ElementAt(index);
        }
    }
}
