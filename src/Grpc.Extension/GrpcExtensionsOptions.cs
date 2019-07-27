using Grpc.Core;
using System;
using System.Collections.Generic;

namespace Grpc.Extension
{
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
        /// 生成proto文件的c#命名空间
        /// </summary>
        public string ProtoNameSpace { get; set; }
        /// <summary>
        /// 是否为基础服务生成proto文件
        /// </summary>
        public bool GenBaseServiceProtoEnable = false;
        /// <summary>
        /// proto的message可能的开头的关键字
        /// </summary>
        public List<string> ProtoMsgStartWithKeywords { get; set; } = new List<string> { "message", "enum" };

        public bool GlobalSaveResponseEnable { get; set; } = false;

        public List<string> FillPropExcludePrefixs { get; set; } = new List<string> { "Google." };
    }
}
