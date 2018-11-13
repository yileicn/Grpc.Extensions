using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Configuration;

namespace FM.GrpcDashboard
{
    public class ConsulService
    {
        IConfiguration _config;
        public ConsulService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// 获取所有Node
        /// </summary>
        /// <returns></returns>
        public async Task<List<Node>> GetAllNode()
        {
            using (var client = new ConsulClient(conf => conf.Address = new Uri(_config["Consul"])))
            {
                var resp = await client.Catalog.Nodes();
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return resp.Response.ToList();
                }
                return null;
            }
        }

        /// <summary>
        /// 获取所有的consul服务
        /// </summary>
        public async Task<List<AgentService>> GetAllServices()
        {
            using (var client = new ConsulClient(conf => conf.Address = new Uri(_config["Consul"])))
            {
                var resp = await client.Agent.Services();
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return resp.Response.Values.ToList();
                }
                return null;
            }
        }
        /// <summary>
        /// 获取服务信息
        /// </summary>
        public async Task<List<AgentService>> GetService(string serviceName)
        {
            using (var client = new ConsulClient(conf => conf.Address = new Uri(_config["Consul"])))
            {
                var resp = await client.Health.Service(serviceName, null, true);
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return resp.Response.Select(q => q.Service).ToList();
                }
                return null;
            }
        }
        /// <summary>
        /// 服务反注册
        /// </summary>
        public async Task UnRegService(string serviceName)
        {
            using (var client = new ConsulClient(conf => conf.Address = new Uri(_config["Consul"])))
            {
                var services = (await client.Health.Service(serviceName)).Response;
                var sids = services.Select(q => q.Service.ID).ToList();
                foreach (var sid in sids)
                {
                    var res = client.Agent.ServiceDeregister(sid).Result;
                }
            }
        }
    }
}
