using Grpc.Extension.Model;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Grpc.Extension.Internal
{
    internal class ProtoGenerator
    {
        /// <summary>
        /// 添加proto
        /// </summary>
        internal static void AddProto<TEntity>(string entityName)
        {
            if (!ProtoMethodInfo.Protos.ContainsKey(entityName))
            {
                ProtoMethodInfo.Protos.TryAdd(entityName, ProtoGenerator.FilterHead(Serializer.GetProto<TEntity>(ProtoBuf.Meta.ProtoSyntax.Proto3)));
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
        internal static string FilterHead(string proto)
        {
            var lines = new List<string>();
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(proto)))
            using (var sr = new StreamReader(ms))
            {
                var readEnable = false;
                while (sr.Peek() > 0)
                {
                    var line = sr.ReadLine();
                    if (GrpcExtensionsOptions.Instance.ProtoMsgStartWithKeywords.Any(q => line.StartsWith(q)))
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
        private static string GenGrpcMessageProto(string pkgName, List<string> msgProtos)
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
            sb.AppendLine();
            sb.AppendLine(Environment.NewLine);

            var sbMsg = new StringBuilder();
            foreach (var proto in msgProtos)
            {
                sbMsg.Append(proto);
                sbMsg.AppendLine(Environment.NewLine);
            }
            var msg = sbMsg.ToString();
            var msgMapProtos = new Dictionary<string, List<string>>();
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(msg)))
            using (var sr = new StreamReader(ms))
            {
                bool readEnable = false;
                var msgName = "";
                var lines = new List<string>();
                while (sr.Peek() > 0)
                {
                    var line = sr.ReadLine();
                    if (GrpcExtensionsOptions.Instance.ProtoMsgStartWithKeywords.Any(q => line.StartsWith(q)))
                    {
                        readEnable = true;
                        msgName = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1];
                        lines = new List<string>();
                    }
                    if (readEnable)
                    {
                        lines.Add(line);
                    }
                    if (line.StartsWith("}"))
                    {
                        readEnable = false;
                        if (!msgMapProtos.ContainsKey(msgName))
                        {
                            msgMapProtos.Add(msgName, lines.Select(q => q).ToList());
                        }
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
        private static string GenGrpcServiceProto(string msgProtoFile, string pkgName, string srvName, List<Tuple<string, string, string>> methodInfo)
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
            sb.AppendLine(string.Format("import \"{0}\";", msgProtoFile));
            sb.AppendLine(Environment.NewLine);
            sb.AppendLine("service " + srvName + " {");

            var template = @"   rpc {0}({1}) returns({2})";
            methodInfo.ForEach(q => sb.AppendLine(Environment.NewLine + string.Format(template, q.Item1, q.Item2, q.Item3) + ";"));

            sb.AppendLine("}");
            return sb.ToString();
        }
        /// <summary>
        /// 生成proto文件
        /// </summary>
        public static void Gen(string dir)
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
                var msgProto = $"{protoName}.message.proto";
                var protoPath = Path.Combine(dir, msgProto);
                if (File.Exists(protoPath))
                {
                    File.Delete(protoPath);
                }
                var msgProtos = new List<string>();
                var rqNames = grp.ToList().Select(q => q.RequestName).ToList();
                var rsNames = grp.ToList().Select(q => q.ResponseName).ToList();
                var msgNames = rqNames.Union(rsNames).Distinct().ToList();
                foreach (var n in msgNames)
                {
                    msgProtos.Add(GetProto(n));
                }
                var msgProtoContent = GenGrpcMessageProto(pkg, msgProtos);
                File.AppendAllText(protoPath, msgProtoContent);
                #endregion

                #region service
                var srvProto = $"{protoName}.service.proto";
                protoPath = Path.Combine(dir, srvProto);
                if (File.Exists(protoPath))
                {
                    File.Delete(protoPath);
                }
                var methodInfos = grp.ToList().Select(m => Tuple.Create(m.MethodName, m.RequestName, m.ResponseName)).ToList();
                var srvProtoContent = GenGrpcServiceProto(msgProto, pkg, srv, methodInfos);
                File.AppendAllText(protoPath, srvProtoContent);
                #endregion
            }
        }
    }
}
