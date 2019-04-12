using Grpc.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Grpc.Extension.Common;
using Grpc.Extension.Model;
using Grpc.Extension.Internal;

namespace Grpc.Extension.BaseService
{
    /// <summary>
    /// GrpcServiceExtension
    /// </summary>
    public static class GrpcServiceExtension
    {
        /// <summary>
        /// 生成Grpc方法（CodeFirst方式，用于生成BaseService）
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="srv"></param>
        /// <param name="methodName"></param>
        /// <param name="package"></param>
        /// <param name="srvName"></param>
        /// <param name="mType"></param>
        /// <returns></returns>
        public static Method<TRequest, TResponse> BuildMethod<TRequest, TResponse>(this IGrpcService srv,
            string methodName, string package = null, string srvName = null, MethodType mType = MethodType.Unary)
        {
            var serviceName = srvName ??
                              GrpcExtensionsOptions.Instance.GlobalService ??
                              srv.GetType().Name;
            var pkg = package ?? GrpcExtensionsOptions.Instance.GlobalPackage;
            if (!string.IsNullOrWhiteSpace(pkg))
            {
                serviceName = $"{pkg}.{serviceName}";
            }
            #region 为生成proto收集信息
            if (!(srv is IGrpcBaseService) || GrpcExtensionsOptions.Instance.GenBaseServiceProtoEnable)
            {
                ProtoInfo.Methods.Add(new ProtoMethodInfo
                {
                    ServiceName = serviceName,
                    MethodName = methodName,
                    RequestName = typeof(TRequest).Name,
                    ResponseName = typeof(TResponse).Name,
                    MethodType = mType
                });
                ProtoGenerator.AddProto<TRequest>(typeof(TRequest).Name);
                ProtoGenerator.AddProto<TResponse>(typeof(TResponse).Name);
            }
            #endregion
            var request = Marshallers.Create<TRequest>((arg) => ProtobufExtensions.Serialize<TRequest>(arg), data => ProtobufExtensions.Deserialize<TRequest>(data));
            var response = Marshallers.Create<TResponse>((arg) => ProtobufExtensions.Serialize<TResponse>(arg), data => ProtobufExtensions.Deserialize<TResponse>(data));
            return new Method<TRequest, TResponse>(mType, serviceName, methodName, request, response);
        }

        /// <summary>
        /// 生成Grpc元数据信息
        /// </summary>
        /// <param name="builder"></param>
        public static void BuildMeta(IDictionary callHandlers)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            //获取Grpc元数据信息
            foreach (DictionaryEntry callHandler in callHandlers)
            {
                //反射获取Handlers
                var hFiled = callHandler.Value.GetFieldValue<Delegate>("handler", bindingFlags);
                var handler = hFiled.Item1;
                var types = hFiled.Item2.DeclaringType.GenericTypeArguments;
                MetaModel.Methods.Add((new MetaMethodModel
                {
                    FullName = callHandler.Key.ToString(),
                    RequestType = types[0],
                    ResponseType = types[1],
                    Handler = handler
                }));                
            }
        }
    }
}
