using Grpc.Core;
using System;
using System.Collections.Generic;

namespace Grpc.Extension
{
    public class GrpcExtensionsOptions
    {
        private static Lazy<GrpcExtensionsOptions> instance = new Lazy<GrpcExtensionsOptions>(() => new GrpcExtensionsOptions(), true);
        public static GrpcExtensionsOptions Instance => instance.Value;

        private GrpcExtensionsOptions()
        {
        }

        public bool GlobalSaveResponseEnable { get; set; } = false;

        public List<string> FillPropExcludePrefixs { get; set; } = new List<string> { "Google." };
    }
}
