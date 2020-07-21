using Consul;
using Grpc.Extension.Abstract;
using Grpc.Extension.Abstract.Discovery;
using Grpc.Extension.Abstract.Model;
using Grpc.Extension.Discovery.Consul.Checks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Grpc.Extension.Discovery.Consul
{
    /// <summary>
    /// Consul服务注册
    /// </summary>
    public class ConsulServiceRegister : IServiceRegister
    {
        private Timer _timerTTL;
        private string _guid;
        private ConsulClient _client;
        private ServiceRegisterModel _model;
        private IConsulCheck _check;

        /// <summary>
        /// Consul服务注册
        /// </summary>
        public ConsulServiceRegister(IConsulCheck check)
        {
            this._guid = Guid.NewGuid().ToString();
            this._check = check;
        }

        /// <summary>
        /// 注册服务到consul
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public async Task RegisterService(ServiceRegisterModel model)
        {
            this._model = model;
            this._client = CreateConsulClient();

            await RegisterServiceCore();
        }

        private async Task RegisterServiceCore()
        {
            var registration = new AgentServiceRegistration()
            {
                ID = GetServiceId(),
                Name = _model.DiscoveryServiceName,
                Tags = _model.DiscoveryServiceTags?.Split(','),
                EnableTagOverride = true,
                Address = _model.ServiceIp,
                Port = _model.ServicePort,
            };
            var check = _check.GetCheck(registration);
            registration.Check = check;
            await _client.Agent.ServiceRegister(registration);

            if (check.Name.StartsWith("ttlcheck"))
            {
                _timerTTL = new Timer(async state => await DoTTL(), null, 0, Timeout.Infinite);
            }
        }

        /// <summary>
        /// 从consul反注册
        /// </summary>
        public Task DeregisterService()
        {
            return _client.Agent.ServiceDeregister(GetServiceId());
        }

        private string GetServiceId()
        {
            return $"{_model.DiscoveryServiceName}-{(_model.ServiceIp)}-{(_model.ServicePort)}-{_guid}";
        }

        private string GetTTLCheckId()
        {
            return $"service:{GetServiceId()}";
        }

        private async Task DoTTL()
        {
            _timerTTL.Change(Timeout.Infinite, Timeout.Infinite);
            try
            {
                await _client.Agent.PassTTL(GetTTLCheckId(), "timer:" + DateTime.Now);
            }
            catch (Exception ex)
            {
                LoggerAccessor.Instance.OnLoggerError(new InternalException(GrpcErrorCode.Internal, "DoTTL", ex));

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
                    await RegisterServiceCore();
                }
            }
            finally
            {
                _timerTTL.Change(TimeSpan.FromSeconds(_model.DiscoveryTTLInterval), TimeSpan.FromSeconds(_model.DiscoveryTTLInterval));
            }
        }

        private ConsulClient CreateConsulClient()
        {
            return new ConsulClient(conf => conf.Address = new Uri(_model.DiscoveryUrl));
        }
    }
}
