using Grpc;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using static Grpc.BaseService;

namespace FM.GrpcDashboard
{
    public class GrpcService
    {
        ILogger _logger;
        IConfiguration _config;
        ConsulService _consulSrv;

        public GrpcService(ILogger<GrpcService> logger, IConfiguration config, ConsulService consulSrv)
        {
            _logger = logger;
            _config = config;
            _consulSrv = consulSrv;
        }
        /// <summary>
        /// 获取服务基本信息
        /// </summary>
        public async Task<InfoRS> GetInfo(string address, int port)
        {
            var channel = new Channel(address, port, ChannelCredentials.Insecure);
            try
            {
                var client = new BaseServiceClient(channel);
                return client.Info(new InfoRQ { MethodName = "" }, deadline: DateTime.UtcNow.AddSeconds(_config.GetValue<int>("GrpcTimeout")));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
            finally
            {
                await channel.ShutdownAsync();
            }
        }
        /// <summary>
        /// 截流
        /// </summary>
        public Tuple<bool, string> Throttle(string serviceName, string methodName, bool isThrottle)
        {
            var srv = _consulSrv.GetService(serviceName).Result;
            if(srv == null || srv.Count == 0)
            {
                return Tuple.Create(false, $"consul中找不到服务（{serviceName}）");
            }

            var result = true;
            var msg = "";
            foreach (var item in srv)
            {
                var channel = new Channel(item.Address, item.Port, ChannelCredentials.Insecure);
                try
                {
                    var client = new BaseServiceClient(channel);
                    var res = client.AddDelThrottle(new AddDelThrottleRQ
                    {
                        MethodName = methodName,
                        IsDel = !isThrottle
                    }, deadline: DateTime.UtcNow.AddSeconds(_config.GetValue<int>("GrpcTimeout")));
                }
                catch (Exception ex)
                {
                    result = false;
                    msg += $"{item.Address};{item.Port}执行失败" + Environment.NewLine;
                    _logger.LogError(ex, ex.Message);
                }
                finally
                {
                    channel.ShutdownAsync().Wait();
                }
            }
            return Tuple.Create(result, msg);
        }
        /// <summary>
        /// 保持响应
        /// </summary>
        public Tuple<bool, string> SaveResponse(string serviceName, string methodName, bool isSaveResponse)
        {
            var srv = _consulSrv.GetService(serviceName).Result;
            if (srv == null || srv.Count == 0)
            {
                return Tuple.Create(false, $"consul中找不到服务（{serviceName}）");
            }

            var result = true;
            var msg = "";
            foreach (var item in srv)
            {
                var channel = new Channel(item.Address, item.Port, ChannelCredentials.Insecure);
                try
                {
                    var client = new BaseServiceClient(channel);
                    var res = client.AddDelSaveResponseEnable(new AddDelSaveResponseEnableRQ
                    {
                        MethodName = methodName,
                        IsDel = !isSaveResponse
                    }, deadline: DateTime.UtcNow.AddSeconds(_config.GetValue<int>("GrpcTimeout")));
                }
                catch (Exception ex)
                {
                    result = false;
                    msg += $"{item.Address};{item.Port}执行失败" + Environment.NewLine;
                    _logger.LogError(ex, ex.Message);
                }
                finally
                {
                    channel.ShutdownAsync().Wait();
                }
            }
            return Tuple.Create(result, msg);
        }
        /// <summary>
        /// 获取方法信息
        /// </summary>
        public MethodInfoRS GetMethodInfo(string endpoint, string methodName)
        {
            var channel = new Channel(endpoint, ChannelCredentials.Insecure);
            try
            {
                var client = new BaseServiceClient(channel);
                return client.MethodInfo(new MethodInfoRQ
                {
                    FullName = methodName
                }, deadline: DateTime.UtcNow.AddSeconds(_config.GetValue<int>("GrpcTimeout")));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return null;
            }
            finally
            {
                channel.ShutdownAsync().Wait();
            }
        }
        /// <summary>
        /// grpc方法调用
        /// </summary>
        public string MethodInvoke(string endpoint, string methodName, string requestJson)
        {
            var channel = new Channel(endpoint, ChannelCredentials.Insecure);
            try
            {
                var client = new BaseServiceClient(channel);
                return client.MethodInvoke(new MethodInvokeRQ
                {
                    FullName = methodName,
                    RequestJson = requestJson
                }/*, deadline: DateTime.UtcNow.AddSeconds(_config.GetValue<int>("GrpcTimeout"))*/).ResponseJson;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ex.ToString();
            }
            finally
            {
                channel.ShutdownAsync().Wait();
            }
        }
    }
}
