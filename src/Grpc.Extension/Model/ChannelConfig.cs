using System;

namespace Grpc.Extension.Model
{
    /// <summary>
    /// ChannelConfig
    /// </summary>
    internal class ChannelConfig
    {
        public string DiscoveryUrl { get; set; }

        public string DiscoveryServiceName { get; set; }

        public string DirectEndpoint { get; set; }

        public bool UseDirect { get; set; }

        public string GrpcServiceName { get; set; }
    }
}
