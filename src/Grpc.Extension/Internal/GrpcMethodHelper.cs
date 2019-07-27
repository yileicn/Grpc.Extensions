using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Extension.BaseService;
using Grpc.Extension.Common;

namespace Grpc.Extension.Internal
{
    // ReSharper disable once IdentifierTypo
    internal static class GrpcMethodHelper
    {
        // ReSharper disable once InconsistentNaming
        private static readonly MethodInfo buildMethod;
        // ReSharper disable once InconsistentNaming
        private static readonly MethodInfo unaryAddMethod;

        // ReSharper disable once IdentifierTypo
        static GrpcMethodHelper()
        {
            buildMethod = typeof(GrpcServiceExtension).GetMethod("BuildMethod");
            var methods = typeof(ServerServiceDefinition.Builder).GetMethods().Where(p => p.Name == "AddMethod");
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length != 2) continue;
                if (parameters[1].ParameterType.Name.Contains("UnaryServerMethod"))
                {
                    unaryAddMethod = method;
                    break;
                }
            }
        }

        /// <summary>
        /// 自动注册服务方法
        /// </summary>
        /// <param name="srv"></param>
        /// <param name="builder"></param>
        /// <param name="serviceName"></param>
        public static void AutoRegisterMethod(IGrpcService srv, ServerServiceDefinition.Builder builder, string package = null, string serviceName = null)
        {
            var methods = srv.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                if (method.ReturnType == typeof(void) || method.ReturnType.BaseType != typeof(Task)) continue;
                var parameters = method.GetParameters();
                if (parameters.Length != 2 || parameters[1].ParameterType != typeof(ServerCallContext) ||
                    method.CustomAttributes.Any(x => x.AttributeType == typeof(NotGrpcMethodAttribute))) continue;

                Type inputType = parameters[0].ParameterType;
                Type outputType = method.ReturnType.GenericTypeArguments[0];

                var buildMethodResult = buildMethod.MakeGenericMethod(inputType, outputType)
                    .Invoke(null, new object[] { srv, method.Name, package, serviceName, MethodType.Unary });

                Delegate unaryDelegate = method.CreateDelegate(typeof(UnaryServerMethod<,>)
                    .MakeGenericType(inputType, outputType), method.IsStatic ? null : srv);

                unaryAddMethod.MakeGenericMethod(inputType, outputType).Invoke(builder, new[] { buildMethodResult, unaryDelegate });
            }
        }
    }

    /// <summary>
    /// 非Grpc方法
    /// </summary>
    public sealed class NotGrpcMethodAttribute : Attribute { }
}
