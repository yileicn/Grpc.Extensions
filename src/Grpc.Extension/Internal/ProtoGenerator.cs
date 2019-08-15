using Grpc.Extension.Model;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Grpc.Extension.Internal
{
    internal static class ProtoGenerator
    {
        /// <summary>
        /// proto的message可能的开头的关键字
        /// </summary>
        private static List<string> protoMsgStartWithKeywords { get; set; } = new List<string> { "message", "enum" };

        /// <summary>
        /// 添加proto
        /// </summary>
        internal static void AddProto<TEntity>(string entityName)
        {
            if (!ProtoMethodInfo.Protos.ContainsKey(entityName))
            {
                var msg = Serializer.GetProto<TEntity>(ProtoBuf.Meta.ProtoSyntax.Proto3);
                ProtoMethodInfo.Protos.TryAdd(entityName, msg.FilterHead().AddMessageComment<TEntity>());
            }
        }
        /// <summary>
        /// 获取实体对应的proto
        /// </summary>
        internal static string GetProto(string entityName)
        {
            var rst = ProtoMethodInfo.Protos.TryGetValue(entityName, out string proto);
            return rst ? proto : null;
        }

        /// <summary>
        /// 过滤头部 只保留message部分
        /// </summary>
        internal static string FilterHead(this string proto)
        {
            var lines = new List<string>();
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(proto)))
            using (var sr = new StreamReader(ms))
            {
                var readEnable = false;
                while (sr.Peek() > 0)
                {
                    var line = sr.ReadLine();
                    if (protoMsgStartWithKeywords.Any(q => line.StartsWith(q)))
                    {
                        readEnable = true;
                    }
                    if (readEnable)
                    {
                        lines.Add(line);
                    }
                }
            }
            return string.Join(Environment.NewLine, lines);
        }
        /// <summary>
        /// 生成grpc的message的proto内容
        /// </summary>
        private static string GenGrpcMessageProto(string pkgName, List<string> msgProtos, bool spiltProto)
        {
            var sb = new StringBuilder();
            if (spiltProto)
            {
                sb.AppendLine("syntax = \"proto3\";");
                if (!string.IsNullOrWhiteSpace(GrpcExtensionsOptions.Instance.ProtoNameSpace))
                {
                    sb.AppendLine("option csharp_namespace = \"" + GrpcExtensionsOptions.Instance.ProtoNameSpace.Trim() + "\";");
                }
                if (!string.IsNullOrWhiteSpace(pkgName))
                {
                    sb.AppendLine($"package {pkgName.Trim()};");
                }
            }
            sb.AppendLine();
            sb.AppendLine(Environment.NewLine);

            //过滤重复的message
            var sbMsg = new StringBuilder();
            foreach (var proto in msgProtos)
            {
                sbMsg.AppendLine(proto);
            }
            var msg = sbMsg.ToString();
            var msgMapProtos = new Dictionary<string, List<string>>();
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(msg)))
            using (var sr = new StreamReader(ms))
            {
                var msgName = "";
                var lines = new List<string>();
                while (sr.Peek() > 0)
                {
                    var line = sr.ReadLine();
                    if (protoMsgStartWithKeywords.Any(q => line.StartsWith(q)))
                    {
                        msgName = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1];
                    }

                    lines.Add(line);

                    if (line.StartsWith("}"))
                    {
                        if (!msgMapProtos.ContainsKey(msgName))
                        {
                            msgMapProtos.Add(msgName, lines.Select(q => q).ToList());
                        }
                        lines.Clear();
                    }
                }
            }
            msg = string.Join(Environment.NewLine + Environment.NewLine, msgMapProtos.Select(q => string.Join(Environment.NewLine, q.Value)));
            sb.Append(msg);
            return sb.ToString();
        }
        /// <summary>
        /// 生成grpc的service的proto内容
        /// </summary>
        private static string GenGrpcServiceProto(string msgProtoName, string pkgName, string srvName, List<ProtoMethodInfo> methodInfo, bool spiltProto)
        {
            var sb = new StringBuilder();
            sb.AppendLine("syntax = \"proto3\";");
            if (!string.IsNullOrWhiteSpace(GrpcExtensionsOptions.Instance.ProtoNameSpace))
            {
                sb.AppendLine("option csharp_namespace = \"" + GrpcExtensionsOptions.Instance.ProtoNameSpace.Trim() + "\";");
            }
            if (!string.IsNullOrWhiteSpace(pkgName))
            {
                sb.AppendLine($"package {pkgName.Trim()};");
            }
            if (spiltProto)
            {
                sb.AppendLine(string.Format("import \"{0}\";", msgProtoName));
            }
            sb.AppendLine(Environment.NewLine);
            sb.AppendLine("service " + srvName + " {");

            var template = @"   rpc {0}({1}) returns({2})";
            methodInfo.ForEach(q => {
                var requestName = q.RequestName;
                var responseName = q.ResponseName;
                switch (q.MethodType)
                {
                    case Core.MethodType.Unary:
                        break;
                    case Core.MethodType.ClientStreaming:
                        requestName = "stream " + requestName;
                        break;
                    case Core.MethodType.ServerStreaming:
                        responseName = "stream " + responseName;
                        break;
                    case Core.MethodType.DuplexStreaming:
                        requestName = "stream " + requestName;
                        responseName = "stream " + responseName;
                        break;
                }
                ProtoCommentGenerator.AddServiceComment(q,sb);
                sb.AppendLine(string.Format(template, q.MethodName, requestName, responseName) + ";" + Environment.NewLine);
            });

            sb.AppendLine("}");
            return sb.ToString();
        }
        /// <summary>
        /// 生成proto文件
        /// </summary>
        public static void Gen(string dir,bool spiltProto)
        {
            if (ProtoInfo.Methods == null || ProtoInfo.Methods.Count == 0) return;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            foreach (var grp in ProtoInfo.Methods.GroupBy(q => q.ServiceName))
            {
                var arr = grp.Key.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length > 2) continue;
                var pkg = arr.Length == 2 ? arr[0] : null;
                var srv = arr.Length == 2 ? arr[1] : arr[0];

                #region message
                var protoName = grp.Key;
                var msgProtoName = $"{protoName}{(spiltProto ? ".message":"")}.proto";
                var msgProtoPath = Path.Combine(dir, msgProtoName);
                var msgProtos = new List<string>();
                var rqNames = grp.ToList().Select(q => q.RequestName).ToList();
                var rsNames = grp.ToList().Select(q => q.ResponseName).ToList();
                var msgNames = rqNames.Union(rsNames).Distinct().ToList();
                foreach (var n in msgNames)
                {
                    msgProtos.Add(GetProto(n));
                }
                var msgProtoContent = GenGrpcMessageProto(pkg, msgProtos, spiltProto);
                #endregion

                #region service
                var srvProtoName = $"{protoName}{(spiltProto ? ".service":"")}.proto";
                var srvProtoPath = Path.Combine(dir, srvProtoName);
                var methodInfos = grp.ToList();
                var srvProtoContent = GenGrpcServiceProto(msgProtoName, pkg, srv, methodInfos, spiltProto);
                #endregion

                //是否拆分message和service协议
                if (spiltProto)
                {
                    //写message协议文件
                    if (File.Exists(msgProtoPath)) File.Delete(msgProtoPath);
                    File.AppendAllText(msgProtoPath, msgProtoContent);
                    //写service协议文件
                    if (File.Exists(srvProtoPath)) File.Delete(srvProtoPath);
                    File.AppendAllText(srvProtoPath, srvProtoContent);

                }
                else
                {
                    var protoPath = Path.Combine(dir, $"{protoName}.proto");
                    //写协议文件
                    if (File.Exists(protoPath)) File.Delete(protoPath);
                    File.AppendAllText(protoPath, srvProtoContent);
                    File.AppendAllText(protoPath, msgProtoContent);
                }
            }
        }
    }
}
