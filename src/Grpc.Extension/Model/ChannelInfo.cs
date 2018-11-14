using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;

namespace Grpc.Extension.Model
{
    public class ChannelInfo
    {
        public string ConsulServiceName { get; set; }
        public Channel Channel { get; set; }
    }
}
