using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Reflection;
using ProtoBuf;
using Grpc.Extension.BaseService.Model;

namespace Grpc.Extension.Common.Internal
{
    internal static class ProtoCommentGenerator
    {
        //Xml文档注释
        static List<XmlCommentInfo> xmlComments = new List<XmlCommentInfo>();
        //ProtoType
        static List<Type> protoTypes;
        static ProtoCommentGenerator()
        {
            //加载注释xml文件
            var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.xml");
            var assembliyNames = new List<string>();
            foreach (var file in files)
            {
                var xe = XElement.Load(file);
                //检查是否为注释xml文件
                if (xe.Element("assembly") == null || xe.Element("members") == null) continue;
                assembliyNames.Add(xe.Element("assembly").Value);
                foreach (var item in xe.Element("members").Elements())
                {
                    var name = item.Attribute("name")?.Value;
                    var nameArr = name?.Split(':');
                    if (name != null && nameArr.Length > 1)
                    {
                        xmlComments.Add(new XmlCommentInfo()
                        {
                            FullName = nameArr[1],
                            Type = nameArr[0],
                            Summary = item.Element("summary")?.Value?.Trim()
                        });
                    }
                }
            }

            //初始化protoTypes
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(p => assembliyNames.Contains(p.GetName().Name));
            protoTypes = assemblies.SelectMany(p => p.GetTypes().Where(t => t.GetCustomAttribute<ProtoContractAttribute>() != null)).ToList();
        }

        /// <summary>
        /// 获取注释集合
        /// </summary>
        /// <param name="types"></param>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public static Dictionary<string,string> GetComments(string[] types,string fullName)
        {
            return xmlComments.Where(p => types.Contains(p.Type) && p.FullName.StartsWith(fullName)).ToDictionary(p => p.FullName, p => p.Summary);
        }

        /// <summary>
        /// 获取注释
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public static string GetComment(string type, string fullName)
        {
            return xmlComments.FirstOrDefault(p => p.Type == type && p.FullName.StartsWith(fullName))?.Summary;
        }

        /// <summary>
        /// 给Message加入注释
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="proto"></param>
        /// <returns></returns>
        public static string AddMessageComment<TEntity>(this string proto)
        {
            var dicComment = new Dictionary<string,string>();

            var lines = new List<string>();
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(proto)))
            using (var sr = new StreamReader(ms))
            {
                while (sr.Peek() > 0)
                {
                    var line = sr.ReadLine();
                    var lineArr = line.Split(new string[]{ " ", "repeated" },StringSplitOptions.RemoveEmptyEntries);
                    if (lineArr.Length > 1)
                    {
                        var typeName = lineArr[1];//message和enum后的类型名
                        if (ProtoGenerator.protoMsgStartWithKeywords.Any(q => line.StartsWith(q)))
                        {
                            var fullName = GetProtoTypeFullName<TEntity>(typeName);
                            if(!string.IsNullOrEmpty(fullName)) dicComment = GetComments(new string[] { "T", "P","F" }, fullName);
                        }
                        var propertyName = lineArr[1];//属性名
                        var comment = dicComment.FirstOrDefault(p => p.Key.EndsWith("." + propertyName)).Value;
                        if (!string.IsNullOrWhiteSpace(comment))
                        {
                            if (line.EndsWith(";"))
                            {
                                lines.Add(AddComment(comment, "   "));
                            }
                            else
                            {
                                lines.Add(AddComment(comment));
                            }
                            
                        }
                    }
                    lines.Add(line);
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        //添加注释(多行注释)
        private static string AddComment(string comment, string prefix = "")
        {
            var arr = comment.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(Environment.NewLine, arr.Select(p => $"{prefix}//{p.TrimStart()}"));
        }

        /// <summary>
        /// 根据名字获取ProtoType的FullName
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetProtoTypeFullName<TEntity>(string name)
        {
            //判断TEntity是否就是要获取的类型
            if (typeof(TEntity).Name == name) return typeof(TEntity).FullName;
            //从protoTypes里获取
            var type = protoTypes.Where(t => t.Name == name).FirstOrDefault();
            return type?.FullName;
        }

        /// <summary>
        /// 给Service加入注释
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="sb"></param>
        public static void AddServiceComment(ProtoMethodInfo proto,StringBuilder sb)
        {
            var comment = string.Empty;
            var handler = MetaModel.Methods.FirstOrDefault(p => p.FullName == proto.FullName)?.Handler;
            if (handler != null)
            {
                var fullName = handler.Method.GetPropertyValue<string>("FullName", BindingFlags.Instance | BindingFlags.NonPublic);
                //将方法的FullName转换成注释的FullName
                var xmlFullName = fullName.Replace(" ", "").Replace("`1[", "{").Replace("]", "}");
                comment = GetComment("M", xmlFullName);
                if (!string.IsNullOrWhiteSpace(comment))
                {
                    sb.AppendLine(AddComment(comment, "   "));
                }
            }
        }
    }
}
