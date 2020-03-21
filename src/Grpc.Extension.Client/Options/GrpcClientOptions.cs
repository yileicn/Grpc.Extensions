using Grpc.Extension.Abstract.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.Client
{
    /// <summary>
    /// GrpcClientOptions
    /// </summary>
    public class GrpcClientOptions
    {
        private static readonly GrpcClientOptions _instance = new GrpcClientOptions();
        /// <summary>
        /// Instance
        /// </summary>
        public static GrpcClientOptions Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// 服务发现地址(http://192.168.8.6:8500)
        /// </summary>
        public string DiscoveryUrl { get; set; }

        /// <summary>
        /// 服务地址缓存时间(秒)
        /// </summary>
        public int ServiceAddressCacheTime { get; set; } = 10;

        /// <summary>
        /// 默认错误码
        /// </summary>
        public int DefaultErrorCode
        {
            get { return GrpcErrorCode.DefaultErrorCode; }
            set { GrpcErrorCode.DefaultErrorCode = value; }
        }

        /// <summary>
        /// JaegerOptions
        /// </summary>
        public JaegerOptions Jaeger { get; set; }

        /// <summary>
        /// Grpc客户端调用超时时间(单位:秒)
        /// </summary>
        public double GrpcCallTimeOut { get; set; } = 10;
    }

}
