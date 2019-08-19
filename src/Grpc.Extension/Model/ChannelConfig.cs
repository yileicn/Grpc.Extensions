using Grpc.Core;
using System.Collections.Generic;

namespace Grpc.Extension.Model
{
    /// <summary>
    /// ChannelConfig
    /// </summary>
    internal class ChannelConfig
    {
        /// <summary>
        /// Discovery的服务器地址
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
        /// ChannelOption
        /// </summary>
        public IEnumerable<ChannelOption> ChannelOptions { get; set; }

        /// <summary>
        /// GrpcServiceName
        /// </summary>
        internal string GrpcServiceName { get; set; }
    }
}
