using Grpc.Core;
using System;
using System.Linq;
using Grpc.Extension.Common.Internal;
using Grpc.Extension.Common;
using Grpc.Extension.Abstract.Model;
using Grpc.Extension.Abstract.Discovery;
using Grpc.Extension.BaseService.Model;

namespace Grpc.Extension
{
    /// <summary>
    /// Grpc扩展
    /// </summary>
    public static class GrpcExtensions
    {
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
                
                //检查服务注册配制
                if (string.IsNullOrWhiteSpace(GrpcServerOptions.Instance.DiscoveryUrl))
                    throw new ArgumentException("GrpcServer:DiscoveryUrl is null");
                if (string.IsNullOrWhiteSpace(GrpcServerOptions.Instance.DiscoveryServiceName))
                    throw new ArgumentException("GrpcServer:DiscoveryServiceName is null");

                //服务注册
                var serviceRegister = ServiceProviderAccessor.GetService<IServiceRegister>();
                Console.WriteLine($"use {serviceRegister.GetType().Name} register");
                Console.WriteLine($"    DiscoveryUrl:{GrpcServerOptions.Instance.DiscoveryUrl}");
                Console.WriteLine($"    ServiceName:{GrpcServerOptions.Instance.DiscoveryServiceName}");
                var registerModel = GrpcServerOptions.Instance.ToJson().FromJson<ServiceRegisterModel>();
                registerModel.ServiceIp = ipAndPort.Host;
                registerModel.ServicePort = ipAndPort.BoundPort;
                serviceRegister.RegisterService(registerModel);
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
            //服务反注册
            var serviceRegister = ServiceProviderAccessor.GetService<IServiceRegister>();
            serviceRegister.DeregisterService();
            server.ShutdownAsync().Wait();
            
            return server;
        }
        #endregion
    }
}
