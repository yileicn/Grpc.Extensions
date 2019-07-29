using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension
{
    public class JaegerOptions
    {
        /// <summary>
        /// 服务名
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// AgentIp
        /// </summary>
        public string AgentIp { get; set; }

        /// <summary>
        /// AgentPort
        /// </summary>
        public int AgentPort { get; set; }
    }
}
