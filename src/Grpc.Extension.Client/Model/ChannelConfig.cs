using Grpc.Core;
using System.Collections.Generic;

namespace Grpc.Extension.Client.Model
{
    /// <summary>
    /// ChannelConfig
    /// </summary>
    public class ChannelConfig
    {
        /// <summary>
        /// Discovery的服务器地址(http://192.168.8.6:8500)
        /// </summary>
        public string DiscoveryUrl { get; set; }

        /// <summary>
        /// Discovery上客户端服务名字
        /// </summary>
        public string DiscoveryServiceName { get; set; }

        /// <summary>
        /// 直接服务地址，不用服务现
        /// </summary>
        public string DirectEndpoint { get; set; }

        /// <summary>
        /// 是否使用直接服务地址
        /// </summary>
        public bool UseDirect { get; set; }

        /// <summary>
        /// Discovery上客户端服务Tag(可用于版本标记)
        /// </summary>
        public string DiscoveryServiceTag { get; set; }

        /// <summary>
        /// ChannelOption
        /// </summary>
        public IEnumerable<ChannelOption> ChannelOptions { get; set; }

        /// <summary>
        /// GrpcServiceName
        /// </summary>
        internal string GrpcServiceName { get; set; }
    }
}
