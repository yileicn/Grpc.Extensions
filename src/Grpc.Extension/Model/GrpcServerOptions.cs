using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.Model
{
    public class GrpcServerOptions
    {
        private static readonly GrpcServerOptions _instance = new GrpcServerOptions();
        public static GrpcServerOptions Instance
        {
            get { return _instance; }
        }

        public string ServiceAddress { get; set; }

        public string ConsulUrl { get; set; }

        public string ConsulServiceName { get; set; }

        public string ConsulTags { get; set; }

        public int ConsulTTLInterval { get; set; } = 10;
    }
}
