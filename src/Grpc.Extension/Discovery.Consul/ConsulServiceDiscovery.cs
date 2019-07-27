using Consul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using Grpc.Extension.Discovery;

namespace Grpc.Extension.Consul
{
    /// <summary>
    /// Consul服务发现
    /// </summary>
    public class ConsulServiceDiscovery : IServiceDiscovery
    {
        private ConcurrentDictionary<string, ConsulClient> _consulClients = new ConcurrentDictionary<string, ConsulClient>();

        /// <summary>
        /// Consul服务发现
        /// </summary>
        public ConsulServiceDiscovery()
        {
            
        }

        /// <summary>
        /// 从consul获取可用的节点信息
        /// </summary>
        public List<string> GetEndpoints(string serviceName, string consulUrl)
        {
            var client = CreateConsulClient(consulUrl);
            var res = client.Health.Service(serviceName, "", true).Result;
            return res.Response.Select(q => $"{q.Service.Address}:{q.Service.Port}").ToList();
        }        
        
        private ConsulClient CreateConsulClient(string consulUrl = null)
        {
            consulUrl = !string.IsNullOrWhiteSpace(consulUrl) ? consulUrl : GrpcServerOptions.Instance.DiscoveryUrl;
            return _consulClients.GetOrAdd(consulUrl, (url) => new ConsulClient(conf => conf.Address = new Uri(url)));
        }
    }
}
