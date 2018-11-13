using System;

namespace Grpc.Extension.Model
{
    public class ChannelConfig
    {
        public string ConsulUrl { get; set; }

        public string ConsulServiceName { get; set; }

        public string DirectEndpoint { get; set; }

        public bool UseDirect { get; set; }

        public string GrpcServiceName { get; set; }
    }
}
