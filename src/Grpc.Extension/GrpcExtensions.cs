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
