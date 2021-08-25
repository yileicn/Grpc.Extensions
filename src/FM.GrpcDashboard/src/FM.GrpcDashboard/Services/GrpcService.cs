using Grpc;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Grpc.BaseService;

namespace FM.GrpcDashboard.Services
{
    public class GrpcService : IGrpcReflection
    {
        ILogger _logger;
        IConfiguration _config;
        ConsulService _consulSrv;
        int _grpcTimeout;

        public GrpcService(ILogger<GrpcService> logger, IConfiguration config, ConsulService consulSrv)
        {
            _logger = logger;
            _config = config;
            _consulSrv = consulSrv;
            _grpcTimeout = _config.GetValue<int>("GrpcTimeout");
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
                return await client.InfoAsync(new InfoRQ { MethodName = "" }, 
                    deadline: DateTime.UtcNow.AddSeconds(_grpcTimeout));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw ex;
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
        public async Task<MethodInfoRS> GetMethodInfo(string endpoint, string methodName)
        {
            var channel = new Channel(endpoint, ChannelCredentials.Insecure);
            try
            {
                var client = new BaseServiceClient(channel);
                return await client.MethodInfoAsync(new MethodInfoRQ
                {
                    FullName = methodName
                }, deadline: DateTime.UtcNow.AddSeconds(_grpcTimeout));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw ex;
            }
            finally
            {
                await channel.ShutdownAsync();
            }
        }
        /// <summary>
        /// grpc方法调用
        /// </summary>
        public async Task<string> MethodInvoke(string endpoint, string methodName, string requestJson, Dictionary<string, string> customHeaders)
        {
            var channel = new Channel(endpoint, ChannelCredentials.Insecure);
            try
            {
                var client = new BaseServiceClient(channel);
                var metadata = new Metadata();
                foreach (var item in customHeaders)
                {
                    if (!metadata.Any(p => p.Key == item.Key))
                        metadata.Add(new Metadata.Entry(item.Key, item.Value));
                }
                return (await client.MethodInvokeAsync(new MethodInvokeRQ
                {
                    FullName = methodName,
                    RequestJson = requestJson
                }, metadata
                // , deadline: DateTime.UtcNow.AddSeconds(_grpcTimeout)
                )).ResponseJson;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ex.ToString();
            }
            finally
            {
                await channel.ShutdownAsync();
            }
        }
    }
}
