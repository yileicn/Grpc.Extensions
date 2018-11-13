using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Extension.Common;

namespace Grpc.Extension.BaseService
{
    public static class GrpcServiceExtension
    {
        /// <summary>
        /// 生成Grpc方法（CodeFirst方式，用于生成BaseService）
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="serverServiceDefinition"></param>
        /// <param name="methodName"></param>
        /// <param name="package"></param>
        /// <param name="srvName"></param>
        /// <param name="mType"></param>
        /// <returns></returns>
        public static Method<TRequest, TResponse> BuildMethod<TRequest, TResponse>(this ServerServiceDefinition.Builder serverServiceDefinition,
            string methodName, string package, string srvName, MethodType mType = MethodType.Unary)
        {
            string serviceName = $"{package}.{srvName}";
            var request = Marshallers.Create<TRequest>((arg) => ProtobufExtensions.Serialize<TRequest>(arg), data => ProtobufExtensions.Deserialize<TRequest>(data));
            var response = Marshallers.Create<TResponse>((arg) => ProtobufExtensions.Serialize<TResponse>(arg), data => ProtobufExtensions.Deserialize<TResponse>(data));
            return new Method<TRequest, TResponse>(mType, serviceName, methodName, request, response);
        }
    }
}
