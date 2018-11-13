using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Extension.Filter;
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

namespace Grpc.Extension
{
    public static class GrpcExtensions
    {
        /// <summary>
        /// 使用基础过滤器
        /// </summary>
        /// <param name="serverServiceDefinition"></param>
        /// <returns></returns>
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

            #region 获取Grpc元数据信息
            foreach (DictionaryEntry callHandler in metaCallHandlers)
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
            #endregion

            return metaBuilder.Build();
        }

        /// <summary>
        /// 配制日志
        /// </summary>
        /// <param name="serverServiceDefinition"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static ServerServiceDefinition UseLogger(this ServerServiceDefinition serverServiceDefinition,Action<LoggerAccessor> action)
        {
            action(LoggerAccessor.Instance);
            return serverServiceDefinition;
        }

        /// <summary>
        /// Consul配制
        /// </summary>
        /// <param name="server"></param>
        /// <param name="consulUrl"></param>
        /// <param name="toConsulServiceName"></param>
        /// <param name="maps"></param>
        /// <param name="toConsulTags"></param>
        /// <returns></returns>
        public static Server UseConsulConfig(this Server server, string consulUrl, string toConsulServiceName, List<ChannelMap> maps = null, params string[] toConsulTags)
        {
            GrpcExtensionsOptions.Instance.ConsulUrl = consulUrl;
            GrpcExtensionsOptions.Instance.ToConsulServiceName = toConsulServiceName;
            GrpcExtensionsOptions.Instance.ToConsulTags = toConsulTags;

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

                ConsulManager.Instance.RegisterService();
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
            ConsulManager.Instance.DeregisterService();
            server.ShutdownAsync().Wait();
            return server;
        }
    }
}
