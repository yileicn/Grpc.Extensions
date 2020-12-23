using Consul;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.Discovery.Consul
{
    /// <summary>
    /// consul 1.3版本使用ID正常
    /// consul 1.7版本使用ID报错,Request decode failed: json: unknown field "ID"，使用CheckID正常
    /// </summary>
    public class NewAgentCheckRegistration : AgentCheckRegistration
    {
        public string CheckID { get; set; }
    }
}
