using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Extension.BaseService;
using Grpc.Extension.Common;
using Grpc.Extension.Interceptors;
using Grpc.Extension.Model;

namespace Grpc.Extension.Internal
{
    /// <summary>
    /// ServerBuilder
    /// </summary>
    public class ServerBuilder
    {
        private readonly List<ServerInterceptor> _interceptors = new List<ServerInterceptor>();
        private readonly List<ServerServiceDefinition> _serviceDefinitions = new List<ServerServiceDefinition>();

        /// <summary>
        /// 注入基本配制
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public ServerBuilder UseOptions(Action<GrpcExtensionsOptions> action)
        {
            action(GrpcExtensionsOptions.Instance);
            return this;
        }

        /// <summary>
        /// 注入Grpc,Consul配制
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public ServerBuilder UseGrpcOptions(GrpcServerOptions options)
        {
            GrpcServerOptions.Instance.ServiceAddress = options.ServiceAddress;
            GrpcServerOptions.Instance.ConsulUrl = options.ConsulUrl;
            GrpcServerOptions.Instance.ConsulServiceName = options.ConsulServiceName;
            GrpcServerOptions.Instance.ConsulTags = options.ConsulTags;
            return this;
        }

        /// <summary>
        /// 注入GrpcService
        /// </summary>
        /// <param name="serviceDefinition"></param>
        /// <returns></returns>
        public ServerBuilder UseGrpcService(ServerServiceDefinition serviceDefinition)
        {
            _serviceDefinitions.Add(serviceDefinition);
            return this;
        }

        /// <summary>
        /// 注入GrpcService
        /// </summary>
        /// <param name="grpcServices"></param>
        /// <returns></returns>
        public ServerBuilder UseGrpcService(IEnumerable<IGrpcService> grpcServices)
        {
            var builder = ServerServiceDefinition.CreateBuilder();
            grpcServices.ToList().ForEach(grpc => grpc.RegisterMethod(builder));
            _serviceDefinitions.Add(builder.Build());
            return this;
        }

        /// <summary>
        /// CodeFirst生成proto文件
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public ServerBuilder UseProtoGenerate(string dir)
        {
            ProtoGenerator.Gen(dir);
            return this;
        }

        /// <summary>
        /// 使用DashBoard(提供基础服务)
        /// </summary>
        /// <returns></returns>
        public ServerBuilder UseDashBoard()
        {
            foreach (var serverServiceDefinition in _serviceDefinitions)
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var callHandlers = serverServiceDefinition.GetPropertyValue<IDictionary>("CallHandlers", bindingFlags);
                GrpcServiceExtension.BuildMeta(callHandlers);
            }
            //注册基础服务
            UseGrpcService(new List<IGrpcService> { new CmdService(), new MetaService() });
            return this;
        }

        /// <summary>
        /// 注入服务端中间件
        /// </summary>
        /// <param name="interceptor"></param>
        /// <returns></returns>
        public ServerBuilder UseInterceptor(ServerInterceptor interceptor)
        {
            _interceptors.Add(interceptor);
            return this;
        }

        /// <summary>
        /// 注入服务端中间件
        /// </summary>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        public ServerBuilder UseInterceptor(IEnumerable<ServerInterceptor> interceptors)
        {
            _interceptors.AddRange(interceptors);
            return this;
        }

        /// <summary>
        /// 配制日志
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public ServerBuilder UseLogger(Action<LoggerAccessor> action)
        {
            action(LoggerAccessor.Instance);
            return this;
        }

        /// <summary>
        /// 构建Server
        /// </summary>
        /// <returns></returns>
        public Server Build()
        {
            Server server = new Server();
            //使用拦截器
            var serviceDefinitions = ApplyInterceptor(_serviceDefinitions, _interceptors);
            //添加服务定义
            foreach (var serviceDefinition in serviceDefinitions)
            {
                server.Services.Add(serviceDefinition);
            }
            //添加服务IPAndPort
            var ipPort = NetHelper.GetIPAndPort(GrpcServerOptions.Instance.ServiceAddress);
            server.Ports.Add(new ServerPort(ipPort.Item1, ipPort.Item2, ServerCredentials.Insecure));
            
            return server;
        }

        private static IEnumerable<ServerServiceDefinition> ApplyInterceptor(IEnumerable<ServerServiceDefinition> serviceDefinitions, IEnumerable<Interceptor> interceptors)
        {
            return serviceDefinitions.Select(serviceDefinition => serviceDefinition.Intercept(interceptors.ToArray()));
        }
    }
}
