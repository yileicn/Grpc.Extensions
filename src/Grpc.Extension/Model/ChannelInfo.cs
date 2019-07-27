using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;

namespace Grpc.Extension.Model
{
    internal class ChannelInfo
    {
        public string DiscoveryServiceName { get; set; }
        public Channel Channel { get; set; }
    }
}
