using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Grpc.Extension.Internal;
using Grpc.Extension.Model;

namespace Grpc.Extension.Consul
{
    /// <summary>
    /// Channel统一管理
    /// </summary>
    public class ChannelManager
    {
        private List<ChannelConfig> _configs = new List<ChannelConfig>();
        private ConcurrentDictionary<string, Channel> _channels = new ConcurrentDictionary<string, Channel>();

        private static Lazy<ChannelManager> _instance = new Lazy<ChannelManager>(() => new ChannelManager(), true);
        public static ChannelManager Instance => _instance.Value;
        private ChannelManager()
        {
        }

        public List<ChannelConfig> Configs
        {
            get { return _configs; }
            set { _configs = value; }
        }

        /// <summary>
        /// 根据客户端代理类型获取channel
        /// </summary>
        public Channel GetChannel(string grpcServiceName)
        {
            var map = _configs?.FirstOrDefault(q => q.GrpcServiceName == grpcServiceName?.Trim());
            if (map == null)
            {
                LoggerAccessor.Instance.LoggerError?.Invoke(new Exception($"GetChannel({grpcServiceName ?? ""}) has not exists"));
                return null;
            }
            if (map.UseDirect)
            {
                return GetChannelCore(map.DirectEndpoint);
            }
            else//from consul
            {
                return GetChannelCore(ConsulManager.Instance.GetEndpoint(map.ConsulServiceName, map.ConsulUrl));
            }
        }

        private Channel GetChannelCore(string endpoint)
        {
            var chnl = _channels.GetOrAdd(endpoint, key => new Channel(key, ChannelCredentials.Insecure));
            if (chnl.State == ChannelState.Shutdown || chnl.State == ChannelState.TransientFailure)
            {
                chnl.ShutdownAsync();
                return _channels.AddOrUpdate(endpoint, key => new Channel(key, ChannelCredentials.Insecure), (k, v) => new Channel(k, ChannelCredentials.Insecure));
            }
            else
            {
                return chnl;
            }
        }

        public void Shutdown()
        {
            _channels.Select(q => q.Value).ToList().ForEach(q => q.ShutdownAsync().Wait());
        }
    }
}
