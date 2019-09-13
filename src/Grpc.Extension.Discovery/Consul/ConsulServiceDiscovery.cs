using Consul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using Grpc.Extension.Abstract.Discovery;

namespace Grpc.Extension.Discovery.Consul
{
    /// <summary>
    /// Consul服务发现
    /// </summary>
    public class ConsulServiceDiscovery : IServiceDiscovery
    {
        private ConcurrentDictionary<string, ConsulClient> _consulClients = new ConcurrentDictionary<string, ConsulClient>();

        /// <summary>
        /// 从consul获取可用的节点信息
        /// </summary>
        public List<string> GetEndpoints(string serviceName, string consulUrl, string consulTag)
        {
            var client = CreateConsulClient(consulUrl);
            var res = client.Health.Service(serviceName, consulTag , true).Result;
            return res.Response.Select(q => $"{q.Service.Address}:{q.Service.Port}").ToList();
        }        
        
        private ConsulClient CreateConsulClient(string consulUrl)
        {
            return _consulClients.GetOrAdd(consulUrl, (url) => new ConsulClient(conf => conf.Address = new Uri(url)));
        }
    }
}
