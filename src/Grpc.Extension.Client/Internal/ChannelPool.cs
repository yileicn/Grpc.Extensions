using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Grpc.Extension.Abstract.Model;
using Grpc.Extension.Abstract.Discovery;
using Grpc.Extension.Abstract;
using Grpc.Extension.Client.Model;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Grpc.Extension.Common;

namespace Grpc.Extension.Client.Internal
{
    /// <summary>
    /// Channel统一管理
    /// </summary>
    internal class ChannelPool
    {
        private AtomicConcurrentDictionary<string, ChannelInfo> _channels = new AtomicConcurrentDictionary<string, ChannelInfo>();
        private IServiceDiscovery _serviceDiscovery;
        private ILoadBalancer _loadBalancer;
        private IMemoryCache _memoryCache;
        private GrpcClientOptions _grpcClientOptions;

        /// <summary>
        /// Channel统一管理
        /// </summary>
        /// <param name="serviceDiscovery"></param>
        /// <param name="loadBalancer"></param>
        /// <param name="memoryCache"></param>
        /// <param name="grpcClientOptions"></param>
        public ChannelPool(IServiceDiscovery serviceDiscovery, ILoadBalancer loadBalancer,IMemoryCache memoryCache, IOptions<GrpcClientOptions> grpcClientOptions)
        {
            this._serviceDiscovery = serviceDiscovery;
            this._loadBalancer = loadBalancer;
            this._memoryCache = memoryCache;
            this._grpcClientOptions = grpcClientOptions.Value;
            this._serviceDiscovery.ServiceChanged += ServiceDiscovery_ServiceChanged;
        }

        internal static List<ChannelConfig> Configs { get; set; } = new List<ChannelConfig>();

        /// <summary>
        /// 根据客户端代理类型获取channel
        /// </summary>
        public async Task<Channel> GetChannel(string grpcServiceName)
        {
            var config = Configs?.FirstOrDefault(q => q.GrpcServiceName == grpcServiceName?.Trim());
            if (config == null)
            {
                throw new InternalException(GrpcErrorCode.Internal, $"{grpcServiceName ?? ""} client has not config,please call AddGrpcClient method");
            }
            if (config.UseDirect)
            {
                return await GetChannelCore(config.DirectEndpoint,config);
            }
            else//from discovery
            {
                var discoveryUrl = !string.IsNullOrWhiteSpace(config.DiscoveryUrl) ? config.DiscoveryUrl : _grpcClientOptions.DiscoveryUrl;
                var discoveryServiceTag = !string.IsNullOrWhiteSpace(config.DiscoveryServiceTag) ? config.DiscoveryServiceTag : _grpcClientOptions.DiscoveryServiceTag;
                var endPoint = await GetEndpoint(config.DiscoveryServiceName, discoveryUrl, discoveryServiceTag);
                return await GetChannelCore(endPoint,config);
            }
        }

        /// <summary>
        /// 根据服务名称返回服务地址
        /// </summary>
        private async Task<string> GetEndpoint(string serviceName, string dicoveryUrl, string serviceTag)
        {
            //获取健康的endpoints
            var isCache = true;
            var healthEndpoints = await _memoryCache.GetOrCreateAsync(serviceName, async cacheEntry =>
            {
                isCache = false;
                //cacheEntry.SetAbsoluteExpiration(TimeSpan.FromSeconds(_grpcClientOptions.ServiceAddressCacheTime));
                return await _serviceDiscovery.GetEndpoints(serviceName, dicoveryUrl, serviceTag);
            });
            if (healthEndpoints == null || healthEndpoints.Count == 0)
            {
                throw new InternalException(GrpcErrorCode.Internal,$"get endpoints from discovery of {serviceName} is null");
            }
            //只有重新拉取了健康结点才需要去关闭不健康的Channel
            if (isCache == false) ShutdownErrorChannel(healthEndpoints, serviceName);

            return _loadBalancer.SelectEndpoint(serviceName, healthEndpoints);
        }

        /// <summary>
        /// 服务地址和端口发生改变
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="healthEndpoints"></param>
        private void ServiceDiscovery_ServiceChanged(string serviceName, List<string> healthEndpoints)
        {
            _memoryCache.Set(serviceName, healthEndpoints);
            //关闭不健康的Channel
            ShutdownErrorChannel(healthEndpoints, serviceName);
        }

        private async Task<Channel> GetChannelCore(string endpoint,ChannelConfig config)
        {
            //获取channel，不存在就添加
            var channelInfo = await _channels.GetOrAddAsync(endpoint, async (key) => await CreateChannel(key, config));
            var channel = channelInfo.Channel;

            //检查channel状态
            if (channel.State != ChannelState.Ready)
            {
                //状态异常就关闭后重建
                _ = channel.ShutdownAsync();
                _channels.TryRemove(config.DiscoveryServiceName, out var tmp);
                //新增或者修改channel
                channelInfo = await CreateChannel(endpoint, config);
                return _channels.AddOrUpdate(endpoint, (key) => channelInfo, (key, value) => channelInfo).Channel;
            }
            else
            {
                return channel;
            }
        }

        private async Task<ChannelInfo> CreateChannel(string endPoint, ChannelConfig config)
        {
            var channel = new Channel(endPoint, ChannelCredentials.Insecure, config.ChannelOptions);

            var tryCount = 0;//重试计数
            //检查channel状态
            while (channel.State != ChannelState.Ready)
            {
                try
                {
                    await channel.ConnectAsync(DateTime.UtcNow.AddSeconds(1));
                }
                catch (Exception ex)
                {
                    tryCount++;
                    var exMsg = $"create channel for {config.DiscoveryServiceName} service failed {tryCount},status:{channel.State},endpoint:{endPoint}";
                    var exeption = new InternalException(GrpcErrorCode.Internal, exMsg, ex);
                    if (tryCount > 2)
                    {
                        throw exeption;
                    }
                    else
                    {
                        LoggerAccessor.Instance.OnLoggerError(exeption, LogType.ClientLog);
                    }
                    //重新获取Endpoint,故障转移
                    if (!config.UseDirect)
                    {
                        endPoint = await GetEndpoint(config.DiscoveryServiceName, config.DiscoveryUrl, config.DiscoveryServiceTag);
                        channel = new Channel(endPoint, ChannelCredentials.Insecure);
                    }
                }
            }
            return new ChannelInfo() { DiscoveryServiceName= config.DiscoveryServiceName,Channel = channel};
        }

        /// <summary>
        /// 关闭不健康Channel
        /// </summary>
        /// <param name="healthEndpoints"></param>
        /// <param name="serviceName"></param>
        private void ShutdownErrorChannel(List<string> healthEndpoints,string serviceName)
        {
            //获取错误的channel
            var errorChannel = _channels.Where(p => p.Value.DiscoveryServiceName == serviceName &&
                                                !healthEndpoints.Contains(p.Key)).ToList();
            //关闭并删除错误的channel
            foreach (var channel in errorChannel)
            {
                channel.Value.Channel.ShutdownAsync();
                _channels.TryRemove(channel.Key, out var tmp);
            }
        }

        /// <summary>
        /// 关闭所有Channel
        /// </summary>
        public void Shutdown()
        {
            _channels.Select(q => q.Value).ToList().ForEach(q => q.Channel.ShutdownAsync().Wait());
            _channels.Clear();
        }
    }
}
