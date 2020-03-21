using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension
{
    /// <summary>
    /// JaegerOptions
    /// </summary>
    public class JaegerOptions
    {
        /// <summary>
        /// 是否启用Jaeger
        /// </summary>
        public bool Enable { get; set; } = true;

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

        /// <summary>
        /// 检查参数配制
        /// </summary>
        /// <returns></returns>
        public bool CheckConfig()
        {
            var key = "Jaeger";
            if (this.Enable == false) return false;

            if (string.IsNullOrWhiteSpace(this.ServiceName))
                throw new ArgumentException($"{key}:ServiceName Value cannot be null");

            if (string.IsNullOrWhiteSpace(this.AgentIp))
                throw new ArgumentException($"{key}:AgentIp Value cannot be null");

            if (this.AgentPort == 0)
                throw new ArgumentNullException($"{key}:AgentPort Value cannot be null");

            return true;
        }
    }
}
