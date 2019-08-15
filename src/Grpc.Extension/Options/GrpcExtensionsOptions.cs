using Grpc.Core;
using System;
using System.Collections.Generic;

namespace Grpc.Extension
{
    /// <summary>
    /// GrpcServerOptions
    /// </summary>
    public class GrpcExtensionsOptions
    {
        private static Lazy<GrpcExtensionsOptions> instance = new Lazy<GrpcExtensionsOptions>(() => new GrpcExtensionsOptions(), true);
        internal static GrpcExtensionsOptions Instance => instance.Value;

        private GrpcExtensionsOptions()
        {
        }

        /// <summary>
        /// grpc服务的包名
        /// </summary>
        public string GlobalPackage { get; set; }
        /// <summary>
        /// grpc服务的对外服务名
        /// </summary>
        public string GlobalService { get; set; }
        /// <summary>
        /// 是否输出响应内容
        /// </summary>
        public bool GlobalSaveResponseEnable { get; set; } = false;
        /// <summary>
        /// 生成proto文件的c#命名空间
        /// </summary>
        public string ProtoNameSpace { get; set; }
        /// <summary>
        /// 是否为基础服务生成proto文件
        /// </summary>
        public bool GenBaseServiceProtoEnable = false;
        /// <summary>
        /// 生成Json对象时排除前缀
        /// </summary>
        internal List<string> FillPropExcludePrefixs { get; set; } = new List<string> { "Google." };
    }
}
