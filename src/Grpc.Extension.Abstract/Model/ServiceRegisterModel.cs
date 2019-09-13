using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.Abstract.Model
{
    /// <summary>
    /// 服务注册模型
    /// </summary>
    public class ServiceRegisterModel
    {
        /// <summary>
        /// Grpc服务Ip
        /// </summary>
        public string ServiceIp { get; set; }

        /// <summary>
        /// Grpc服务Port
        /// </summary>
        public int ServicePort { get; set; }

        /// <summary>
        /// 服务注册地址(http://192.168.8.6:8500)
        /// </summary>
        public string DiscoveryUrl { get; set; }

        /// <summary>
        /// 服务注册名
        /// </summary>
        public string DiscoveryServiceName { get; set; }

        /// <summary>
        /// 服务注册Tags
        /// </summary>
        public string DiscoveryServiceTags { get; set; }

        /// <summary>
        /// 服务TTL(秒)
        /// </summary>
        public int DiscoveryTTLInterval { get; set; } = 10;
    }
}
