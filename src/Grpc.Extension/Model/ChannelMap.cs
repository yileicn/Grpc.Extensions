namespace Grpc.Extension.Model
{
    public class ChannelMap
    {
        public string ConsulUrl { get; set; }

        public string ClientProxyFullName { get; set; }

        public string ConsulServiceName { get; set; }

        public string DirectEndpoint { get; set; }

        public bool UseDirect { get; set; }
    }
}
