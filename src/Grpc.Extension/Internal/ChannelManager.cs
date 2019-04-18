using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Grpc.Extension.LoadBalancer;
using Grpc.Extension.Model;
using Grpc.Extension.Common;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Grpc.Extension.Consul;
using Grpc.Extension.Interceptors;
using Grpc.Core.Interceptors;

namespace Grpc.Extension.Internal
{
    /// <summary>
    /// Channel统一管理
    /// </summary>
    public class ChannelManager
    {
        private ConcurrentDictionary<string, ChannelInfo> _channels = new ConcurrentDictionary<string, ChannelInfo>();
        private ConsulManager _consulManager;
        private ILoadBalancer _loadBalancer;

        public ChannelManager(ConsulManager consulManager, ILoadBalancer loadBalancer)
        {
            this._consulManager = consulManager;
            this._loadBalancer = loadBalancer;
        }

        public List<ChannelConfig> Configs { get; set; } = new List<ChannelConfig>();

        /// <summary>
        /// 根据客户端代理类型获取channel
        /// </summary>
        public Channel GetChannel(string grpcServiceName)
        {
            var config = Configs?.FirstOrDefault(q => q.GrpcServiceName == grpcServiceName?.Trim());
            if (config == null)
            {
                LoggerAccessor.Instance.LoggerError?.Invoke(new Exception($"GetChannel({grpcServiceName ?? ""}) has not exists"));
                return null;
            }
            if (config.UseDirect)
            {
                return GetChannelCore(config.DirectEndpoint,config.ConsulServiceName);
            }
            else//from consul
            {
                var endPoint = GetEndpoint(config.ConsulServiceName, config.ConsulUrl);
                return GetChannelCore(endPoint,config.ConsulServiceName);
            }
        }

        /// <summary>
        /// 根据服务名称返回服务地址
        /// </summary>
        public string GetEndpoint(string serviceName, string consulUrl)
        {
            //获取健康的endpoints
            var healthEndpoints = _consulManager.GetEndpointsFromConsul(serviceName, consulUrl);
            if (healthEndpoints == null || healthEndpoints.Count == 0)
            {
                throw new Exception($"get endpoints from consul of {serviceName} is null");
            }
            //获取错误的channel
            var errorChannel = _channels.Where(p => p.Value.ConsulServiceName == serviceName &&
                                                !healthEndpoints.Contains(p.Key)).ToList();
            //关闭并删除错误的channel
            foreach (var channel in errorChannel)
            {
                channel.Value.Channel.ShutdownAsync();
                _channels.TryRemove(channel.Key,out var tmp);
            }
            
            return _loadBalancer.SelectEndpoint(serviceName, healthEndpoints);
        }

        private Channel GetChannelCore(string endpoint,string consulServiceName)
        {
            Func<string, ChannelInfo> addFunc = key =>
                new ChannelInfo()
                {
                    ConsulServiceName = consulServiceName,
                    Channel = new Channel(key, ChannelCredentials.Insecure)
                };
            //获取channel，不存在就添加
            var channel = _channels.GetOrAdd(endpoint, addFunc).Channel;
            //检查channel状态
            if (channel.State == ChannelState.Shutdown || channel.State == ChannelState.TransientFailure)
            {
                //状态异常就关闭后重建
                channel.ShutdownAsync();
                //新增或者修改channel
                return _channels.AddOrUpdate(endpoint, addFunc, (key, value) => new ChannelInfo()
                    {
                        ConsulServiceName = consulServiceName,
                        Channel = new Channel(key, ChannelCredentials.Insecure)
                    }).Channel;
            }
            else
            {
                return channel;
            }
        }

        public void Shutdown()
        {
            _channels.Select(q => q.Value).ToList().ForEach(q => q.Channel.ShutdownAsync().Wait());
        }
    }

    /// <summary>
    /// GrpcClient
    /// </summary>
    public class GrpcClientManager
    {
        private IEnumerable<ClientInterceptor> _clientInterceptors;
        public GrpcClientManager(IEnumerable<ClientInterceptor> clientInterceptors)
        {
            this._clientInterceptors = clientInterceptors;
        }

        public T GetGrpcClient<T>() where T : ClientBase<T>
        {
            var channelManager = GrpcExtensions.ServiceProvider.GetService<ChannelManager>();
            var bindFlags = BindingFlags.Static | BindingFlags.NonPublic;
            var grpcServiceName = typeof(T).DeclaringType.GetFieldValue<string>("__ServiceName", bindFlags);

            var channel = channelManager.GetChannel(grpcServiceName);
            var callInvoker = channel.Intercept(_clientInterceptors.ToArray());
            var client = Activator.CreateInstance(typeof(T), callInvoker);

            return client as T;
        }
    }
}
