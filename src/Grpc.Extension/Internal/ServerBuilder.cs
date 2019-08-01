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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTracing;
using OpenTracing.Util;

namespace Grpc.Extension.Internal
{
    /// <summary>
    /// ServerBuilder
    /// </summary>
    public class ServerBuilder
    {
        private readonly List<ServerInterceptor> _interceptors = new List<ServerInterceptor>();
        private readonly List<ServerServiceDefinition> _serviceDefinitions = new List<ServerServiceDefinition>();
        private readonly List<IGrpcService> _grpcServices = new List<IGrpcService>();

        public ServerBuilder(IServiceProvider serviceProvider,
            IConfiguration conf,
            IEnumerable<ServerInterceptor> serverInterceptors,
            IEnumerable<IGrpcService> grpcServices)
        {
            GrpcExtensions.ServiceProvider = serviceProvider;
            this._grpcServices.AddRange(grpcServices);
            //初始化配制,注入中间件,GrpcService
            this.InitGrpcOptions(conf)//初始化配制
                .UseInterceptor(serverInterceptors);//注入中间件
        }

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
        /// 从配制文件初始化
        /// </summary>
        /// <param name="conf"></param>
        private ServerBuilder InitGrpcOptions(IConfiguration conf)
        {
            var section = conf.GetSection("GrpcServer");
            GrpcServerOptions.Instance.ServiceAddress = section["ServiceAddress"];
            //Discovery配制
            GrpcServerOptions.Instance.DiscoveryUrl = section["DiscoveryUrl"] ?? section["ConsulUrl"];
            GrpcServerOptions.Instance.DiscoveryServiceName = section["DiscoveryServiceName"] ?? section["ConsulServiceName"];
            GrpcServerOptions.Instance.DiscoveryTags = section["DiscoveryTags"] ?? section["ConsulTags"];
            //错误码配制
            if(int.TryParse(section["DefaultErrorCode"], out int defaultErrorCode))
                GrpcServerOptions.Instance.DefaultErrorCode = defaultErrorCode;

            return this;
        }

        /// <summary>
        /// 注入Grpc,Discovery配制
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public ServerBuilder UseGrpcOptions(Action<GrpcServerOptions> options)
        {
            options(GrpcServerOptions.Instance);
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
        /// 注入IGrpcService
        /// </summary>
        /// <returns></returns>
        public ServerBuilder UseGrpcService()
        {
            return UseGrpcService(_grpcServices);
        }

        /// <summary>
        /// 注入IGrpcService
        /// </summary>
        /// <param name="grpcServices"></param>
        /// <returns></returns>
        private ServerBuilder UseGrpcService(IEnumerable<IGrpcService> grpcServices)
        {
            var builder = ServerServiceDefinition.CreateBuilder();
            //grpcServices.ToList().ForEach(grpc => grpc.RegisterMethod(builder));
            grpcServices.ToList().ForEach(grpc => {
                if (grpc is IGrpcBaseService)
                {
                    GrpcMethodHelper.AutoRegisterMethod(grpc, builder, Consts.BaseServicePackage, Consts.BaseServiceName);
                }
                else
                {
                    GrpcMethodHelper.AutoRegisterMethod(grpc, builder);
                }
            });
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
        private ServerBuilder UseInterceptor(ServerInterceptor interceptor)
        {
            _interceptors.Add(interceptor);
            return this;
        }

        /// <summary>
        /// 注入服务端中间件
        /// </summary>
        /// <param name="interceptors"></param>
        /// <returns></returns>
        private ServerBuilder UseInterceptor(IEnumerable<ServerInterceptor> interceptors)
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
        /// 有AddJaeger就使用Jaeger
        /// </summary>
        /// <returns></returns>
        private void CheckUseJaeger()
        {
            var tracer = GrpcExtensions.ServiceProvider.GetService<ITracer>();
            if (tracer != null) GlobalTracer.Register(tracer);
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

            //有AddJaeger就UseJaeger
            this.CheckUseJaeger();

            return server;
        }

        private static IEnumerable<ServerServiceDefinition> ApplyInterceptor(IEnumerable<ServerServiceDefinition> serviceDefinitions, IEnumerable<Interceptor> interceptors)
        {
            return serviceDefinitions.Select(serviceDefinition => serviceDefinition.Intercept(interceptors.ToArray()));
        }
    }
}
