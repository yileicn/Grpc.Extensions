using System;
using System.Linq;
using System.Reflection;
using Grpc.Core;
using Grpc.Extension.Abstract;
using Grpc.Extension.BaseService;
using Grpc.Extension.BaseService.Model;
using Grpc.Extension.Common;
using Grpc.Extension.Common.Internal;

namespace Grpc.Extension.AspNetCore.Internal
{
    // ReSharper disable once IdentifierTypo
    internal static class GrpcMethodHelper
    {
        // ReSharper disable once InconsistentNaming
        private static readonly MethodInfo buildMethod;
        // ReSharper disable once InconsistentNaming
        private static readonly MethodInfo unaryAddMethod;
        private static readonly MethodInfo clientStreamingAddMethod;
        private static readonly MethodInfo serverStreamingAddMethod;
        private static readonly MethodInfo duplexStreamingAddMethod;

        // ReSharper disable once IdentifierTypo
        static GrpcMethodHelper()
        {
            buildMethod = typeof(GrpcMethodHelper).GetMethod("BuildMethod");
            var methods = typeof(ServiceBinderBase).GetMethods().Where(p => p.Name == "AddMethod");
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length != 2) continue;
                if (parameters[1].ParameterType.Name.Contains("UnaryServerMethod"))
                {
                    unaryAddMethod = method;
                }
                else if (parameters[1].ParameterType.Name.Contains("ClientStreamingServerMethod"))
                {
                    clientStreamingAddMethod = method;
                }
                else if (parameters[1].ParameterType.Name.Contains("ServerStreamingServerMethod"))
                {
                    serverStreamingAddMethod = method;
                }
                else if (parameters[1].ParameterType.Name.Contains("DuplexStreamingServerMethod"))
                {
                    duplexStreamingAddMethod = method;
                }
            }
        }

        /// <summary>
        /// 自动注册服务方法
        /// </summary>
        /// <param name="srv"></param>
        /// <param name="serviceBinder"></param>
        /// <param name="package"></param>
        /// <param name="serviceName"></param>
        public static void AutoRegisterMethod(Type srv, ServiceBinderBase serviceBinder, string package = null, string serviceName = null)
        {
            var methods = srv.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            foreach (var method in methods)
            {
                if (!method.ReturnType.Name.StartsWith("Task")) continue;
                var parameters = method.GetParameters();
                if (parameters[parameters.Length-1].ParameterType != typeof(ServerCallContext) ||
                    method.CustomAttributes.Any(x => x.AttributeType == typeof(NotGrpcMethodAttribute))) continue;

                Type inputType = parameters[0].ParameterType;
                Type inputType2 = parameters[1].ParameterType;
                Type outputType = method.ReturnType.IsGenericType ? method.ReturnType.GenericTypeArguments[0] : method.ReturnType;

                var addMethod = unaryAddMethod;
                var serverMethodType = typeof(UnaryServerMethod<,>);
                var methodType = MethodType.Unary;
                var reallyInputType = inputType;
                var reallyOutputType = outputType;

                //非一元方法
                if ((inputType.IsGenericType || inputType2.IsGenericType))
                {
                    if (inputType.Name == "IAsyncStreamReader`1")
                    {
                        reallyInputType = inputType.GenericTypeArguments[0];
                        if (inputType2.Name == "IServerStreamWriter`1")//双向流
                        {
                            addMethod = duplexStreamingAddMethod;
                            methodType = MethodType.DuplexStreaming;
                            serverMethodType = typeof(DuplexStreamingServerMethod<,>);
                            reallyOutputType = inputType2.GenericTypeArguments[0];
                        }
                        else//客户端流
                        {
                            addMethod = clientStreamingAddMethod;
                            methodType = MethodType.ClientStreaming;
                            serverMethodType = typeof(ClientStreamingServerMethod<,>);
                        }
                    }
                    else if (inputType2.Name == "IServerStreamWriter`1")//服务端流
                    {
                        addMethod = serverStreamingAddMethod;
                        methodType = MethodType.ServerStreaming;
                        serverMethodType = typeof(ServerStreamingServerMethod<,>);
                        reallyOutputType = inputType2.GenericTypeArguments[0];
                    }
                }
                var buildMethodResult = buildMethod.MakeGenericMethod(reallyInputType, reallyOutputType)
                    .Invoke(null, new object[] { srv, method.Name, package, serviceName, methodType });
                Delegate serverMethodDelegate = method.CreateDelegate(serverMethodType
                .MakeGenericType(reallyInputType, reallyOutputType), null);
                addMethod.MakeGenericMethod(reallyInputType, reallyOutputType).Invoke(serviceBinder, new[] { buildMethodResult, serverMethodDelegate });
            }
        }

        /// <summary>
        /// 生成Grpc方法（CodeFirst方式）
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="srv"></param>
        /// <param name="methodName"></param>
        /// <param name="package"></param>
        /// <param name="srvName"></param>
        /// <param name="mType"></param>
        /// <returns></returns>
        public static Method<TRequest, TResponse> BuildMethod<TRequest, TResponse>(this Type srv,
            string methodName, string package = null, string srvName = null, MethodType mType = MethodType.Unary)
        {
            var serviceName = srvName ??
                              GrpcExtensionsOptions.Instance.GlobalService ??
                              srv.Name;
            var pkg = package ?? GrpcExtensionsOptions.Instance.GlobalPackage;
            if (!string.IsNullOrWhiteSpace(pkg))
            {
                serviceName = $"{pkg}.{serviceName}";
            }
            #region 为生成proto收集信息
            if (!(typeof(IGrpcBaseService).IsAssignableFrom(srv)) || GrpcExtensionsOptions.Instance.GenBaseServiceProtoEnable)
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
        /// 绑定GrpcService的方法
        /// </summary>
        /// <param name="serviceBinder"></param>
        /// <param name="service"></param>
        public static void BindService(ServiceBinderBase serviceBinder, Type service)
        {
            if (typeof(IGrpcBaseService).IsAssignableFrom(service))
            {
                AutoRegisterMethod(service, serviceBinder, ServerConsts.BaseServicePackage, ServerConsts.BaseServiceName);
            }
            else
            {
                AutoRegisterMethod(service, serviceBinder);
            }
        }
    }
}
