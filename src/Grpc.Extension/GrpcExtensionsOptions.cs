using Grpc.Core;
using System;
using System.Collections.Generic;

namespace Grpc.Extension
{
    public class GrpcExtensionsOptions
    {
        static Lazy<GrpcExtensionsOptions> instance = new Lazy<GrpcExtensionsOptions>(() => new GrpcExtensionsOptions(), true);
        public static GrpcExtensionsOptions Instance
        {
            get { return instance.Value; }
        }
        private GrpcExtensionsOptions()
        {
        }

        public bool GlobalSaveResponseEnable { get; set; } = false;

        public string ConsulUrl { get; set; }

        public string ToConsulServiceName { get; set; }

        public string[] ToConsulTags { get; set; }

        public int ConsulTTLIntervalSeconds { get; set; } = 10;

        public List<string> FillPropExcludePrefixs { get; set; } = new List<string> { "Google." };
    }
}
