using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension
{
    public class GrpcServerOptions
    {
        private static readonly GrpcServerOptions _instance = new GrpcServerOptions();
        internal static GrpcServerOptions Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Grpc服务地址(192.168.*.*:)
        /// </summary>
        public string ServiceAddress { get; set; }

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
        public string DiscoveryTags { get; set; }

        /// <summary>
        /// 服务TTL(秒)
        /// </summary>
        public int DiscoveryTTLInterval { get; set; } = 10;

        /// <summary>
        /// 默认错误码
        /// </summary>
        public int DefaultErrorCode { get; set; } = 1;

        #region 兼容老版本
        /*
        /// <summary>
        /// 服务注册地址(http://192.168.8.6:8500)
        /// </summary>
        [Obsolete("已过期，请使用DiscoveryUrl")]
        public string ConsulUrl { get; set; }

        /// <summary>
        /// 服务注册名
        /// </summary>
        [Obsolete("已过期，请使用DiscoveryServiceName")]
        public string ConsulServiceName { get; set; }

        internal void InitCompatible()
        {
            if (!string.IsNullOrWhiteSpace(ConsulUrl) && string.IsNullOrWhiteSpace(DiscoveryUrl))
            {
                DiscoveryUrl = ConsulUrl;
            }

            if (!string.IsNullOrWhiteSpace(ConsulServiceName) && string.IsNullOrWhiteSpace(DiscoveryServiceName))
            {
                DiscoveryServiceName = ConsulServiceName;
            }
        }
        */
        #endregion
    }

}
