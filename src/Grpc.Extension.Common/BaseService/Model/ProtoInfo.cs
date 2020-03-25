using Grpc.Core;
using ProtoBuf;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Grpc.Extension.BaseService.Model
{
    public class ProtoInfo
    {
        /// <summary>
        /// grpc服务方法信息 用于生成proto文件
        /// </summary>
        public static List<ProtoMethodInfo> Methods { get; internal set; } = new List<ProtoMethodInfo>();
    }

    /// <summary>
    /// 注册到grpc的服务方法信息
    /// </summary>
    public class ProtoMethodInfo
    {
        public string ServiceName { get; set; }

        public string MethodName { get; set; }

        public string RequestName { get; set; }

        public string ResponseName { get; set; }

        public string FullName
        {
            get { return "/" + ServiceName + "/" + MethodName; }
        }

        public MethodType MethodType { get; set; }

        public static ConcurrentDictionary<string, string> Protos = new ConcurrentDictionary<string, string>();
    }
}
