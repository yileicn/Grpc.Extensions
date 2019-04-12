using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Extension.Interceptors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Grpc.Extension.BaseService;
using Grpc.Extension.Model;
using Grpc.Extension.Common;
using Grpc.Extension.Consul;
using Grpc.Extension.Internal;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Grpc.Extension
{
    /// <summary>
    /// Grpc扩展
    /// </summary>
    public static class GrpcExtensions
    {
        internal static IServiceProvider ServiceProvider { get; set; }

        #region 服务端扩展
        /// <summary>
        /// 使用基础过滤器
        /// </summary>
        /// <param name="serverServiceDefinition"></param>
        /// <returns></returns>
        [Obsolete("请使用ServerBuiler")]
        public static ServerServiceDefinition UseBaseInterceptor(this ServerServiceDefinition serverServiceDefinition)
        {
            //性能监控，熔断处理
            return serverServiceDefinition
                .Intercept(new MonitorInterceptor())//性能监控
                .Intercept(new ThrottleInterceptor());//熔断处理
        }

        /// <summary>
        /// 使用DashBoard(提供基础服务)
        /// </summary>
        /// <param name="serverServiceDefinition"></param>
        /// <returns></returns>
        [Obsolete("请使用ServerBuiler")]
        public static ServerServiceDefinition UseDashBoard(this ServerServiceDefinition serverServiceDefinition)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var callHandlers = serverServiceDefinition.GetPropertyValue<IDictionary>("CallHandlers", bindingFlags);

            #region 注册基本服务
            var metaService = new MetaService();
            var metaBuilder = ServerServiceDefinition.CreateBuilder();
            metaService.RegisterMethod(metaBuilder);
            var metaCallHandlers = metaBuilder.GetFieldValue<IDictionary>("callHandlers", bindingFlags).Item1;

            foreach (DictionaryEntry callHandler in callHandlers)
            {
                metaCallHandlers.Add(callHandler.Key, callHandler.Value);
            }
            #endregion

            //生成Grpc元数据信息
            GrpcServiceExtension.BuildMeta(metaCallHandlers);

            return metaBuilder.Build();
        }

        /// <summary>
        /// 注入Grpc配制
        /// </summary>
        /// <param name="server"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        [Obsolete("请使用ServerBuiler")]
        public static Server UseGrpcOptions(this Server server, GrpcServerOptions options)
        {
            GrpcServerOptions.Instance.ServiceAddress = options.ServiceAddress;
            GrpcServerOptions.Instance.ConsulUrl = options.ConsulUrl;
            GrpcServerOptions.Instance.ConsulServiceName = options.ConsulServiceName;
            GrpcServerOptions.Instance.ConsulTags = options.ConsulTags;

            //添加服务IPAndPort
            var ipPort = NetHelper.GetIPAndPort(GrpcServerOptions.Instance.ServiceAddress);
            server.Ports.Add(new ServerPort(ipPort.Item1, ipPort.Item2, ServerCredentials.Insecure));
            return server;
        }

        /// <summary>
        /// 注入GrpcService
        /// </summary>
        /// <param name="server"></param>
        /// <param name="grpcServices"></param>
        /// <returns></returns>
        [Obsolete("请使用ServerBuiler")]
        public static Server UseGrpcService(this Server server, IEnumerable<IGrpcService> grpcServices)
        {
            var builder  = ServerServiceDefinition.CreateBuilder();
            grpcServices.ToList().ForEach(grpc => grpc.RegisterMethod(builder));
            server.Services.Add(builder.Build());
            return server;
        }

        /// <summary>
        /// 使用DashBoard(提供基础服务)
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        [Obsolete("请使用ServerBuiler")]
        public static Server UseDashBoard(this Server server)
        {
            foreach (var serverServiceDefinition in server.Services)
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var callHandlers = serverServiceDefinition.GetPropertyValue<IDictionary>("CallHandlers", bindingFlags);
                GrpcServiceExtension.BuildMeta(callHandlers);
            }
            //注册基础服务
            server.UseGrpcService(new List<IGrpcService> { new CmdService(), new MetaService() });
            return server;
        }

        /// <summary>
        /// 配制日志
        /// </summary>
        /// <param name="server"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        [Obsolete("请使用ServerBuiler")]
        public static Server UseLogger(this Server server, Action<LoggerAccessor> action)
        {
            action(LoggerAccessor.Instance);
            return server;
        }

        /// <summary>
        /// 启动并注册服务
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public static Server StartAndRegisterService(this Server server)
        {
            server.Start();
            var ipAndPort = server.Ports.FirstOrDefault();
            if (ipAndPort != null)
            {
                MetaModel.StartTime = DateTime.Now;
                MetaModel.Ip = ipAndPort.Host;
                MetaModel.Port = ipAndPort.BoundPort;
                Console.WriteLine($"server listening {MetaModel.Ip}:{MetaModel.Port}");
                //注册到Consul
                var consulManager = ServiceProvider.GetService<ConsulManager>();
                consulManager.RegisterService();
            }
            return server;
        }

        /// <summary>
        /// 停止并反注册服务
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public static Server StopAndDeRegisterService(this Server server)
        {
            //从Consul反注册
            var consulManager = ServiceProvider.GetService<ConsulManager>();
            consulManager.DeregisterService();
            server.ShutdownAsync().Wait();
            
            return server;
        }
        #endregion
    }
}
