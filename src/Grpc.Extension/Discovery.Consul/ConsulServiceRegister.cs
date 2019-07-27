using Consul;
using Grpc.Extension.Discovery;
using Grpc.Extension.Internal;
using Grpc.Extension.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Grpc.Extension.Consul
{
    /// <summary>
    /// Consul服务注册
    /// </summary>
    public class ConsulServiceRegister : IServiceRegister
    {
        private bool RegisterEnable => !string.IsNullOrWhiteSpace(GrpcServerOptions.Instance.DiscoveryUrl) &&
                                      !string.IsNullOrWhiteSpace(GrpcServerOptions.Instance.DiscoveryServiceName);

        private Timer _timerTTL;
        private string _guid;
        private ConsulClient _client;

        /// <summary>
        /// Consul服务注册
        /// </summary>
        public ConsulServiceRegister()
        {
            this._guid = Guid.NewGuid().ToString();
            this._client = CreateConsulClient();
        }

        /// <summary>
        /// 注册服务到consul
        /// </summary>
        public void RegisterService()
        {
            if (!RegisterEnable) return;

            RegisterServiceCore();

            //因为公司的consul不支持consul主动检查服务状态，所以启动定时器主动去检测
            _timerTTL = new Timer(state => DoTTL(), null, Timeout.Infinite, Timeout.Infinite);
            DoTTL();
        }

        private void RegisterServiceCore()
        {
            var registration = new AgentServiceRegistration()
            {
                ID = GetServiceId(),
                Name = GrpcServerOptions.Instance.DiscoveryServiceName,
                Tags = GrpcServerOptions.Instance.DiscoveryTags?.Split(','),
                EnableTagOverride = true,
                Address = MetaModel.Ip,
                Port = MetaModel.Port,
                //因为公司的consul不支持consul主动检查服务状态，所以注释掉
                //Check = new AgentServiceCheck
                //{
                //    TCP = $"{MetaModel.Ip}:{MetaModel.Port}",
                //    Interval = TimeSpan.FromSeconds(15),
                //    Status = HealthStatus.Passing,
                //    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
                //}
                //因为公司的consul不支持consul主动检查服务状态，所以主动去TTL consul
                Check = new AgentCheckRegistration
                {
                    ID = GetTTLCheckId(),
                    Name = "ttlcheck",
                    TTL = TimeSpan.FromSeconds(15),
                    Status = HealthStatus.Passing,
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                }
            };
            _client.Agent.ServiceRegister(registration).Wait();
        }

        /// <summary>
        /// 从consul反注册
        /// </summary>
        public void DeregisterService()
        {
            if (!RegisterEnable) return;
            _client.Agent.ServiceDeregister(GetServiceId()).Wait();
        }

        private string GetServiceId()
        {
            return $"{GrpcServerOptions.Instance.DiscoveryServiceName}-{(MetaModel.Ip)}-{(MetaModel.Port)}-{_guid}";
        }

        private string GetTTLCheckId()
        {
            return $"service:{GetServiceId()}";
        }

        private void DoTTL()
        {
            _timerTTL.Change(Timeout.Infinite, Timeout.Infinite);
            try
            {
                _client.Agent.PassTTL(GetTTLCheckId(), "timer:" + DateTime.Now).Wait();
            }
            catch (Exception ex)
            {
                LoggerAccessor.Instance.LoggerError?.Invoke(ex);

                /*
                 * passTTL会出现如下几种情况：
                 * 1. consul服务重启中，ex会显示 connection refused by ip:port
                 *          这种情况下，不去处理，等consul服务重启之后就好了
                 * 2. consul服务重启之后，会丢失之前的service，check，会有如下的错误：
                 *          Unexpected response, status code InternalServerError: CheckID "followme.srv.sms-192.168.3.10-10086-07f21040-0be9-4a73-b0a1-71755c6d6d46:ttlcheck" does not have associated TTL
                 *          在这种情况下，需要处理，重新注册服务，check；     
                 */
                if (ex.ToString().Contains($"CheckID \"{GetTTLCheckId()}\" does not have associated TTL"))
                {
                    RegisterServiceCore();
                }
            }
            finally
            {
                _timerTTL.Change(TimeSpan.FromSeconds(GrpcServerOptions.Instance.DiscoveryTTLInterval), TimeSpan.FromSeconds(GrpcServerOptions.Instance.DiscoveryTTLInterval));
            }
        }

        private ConsulClient CreateConsulClient()
        {
            return new ConsulClient(conf => conf.Address = new Uri(GrpcServerOptions.Instance.DiscoveryUrl));
        }
    }
}
