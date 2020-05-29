using Grpc.Core;
using System;
using System.Linq;
using Grpc.Extension.Common.Internal;
using Grpc.Extension.Common;
using Grpc.Extension.Abstract.Model;
using Grpc.Extension.Abstract.Discovery;
using Grpc.Extension.BaseService.Model;
using Microsoft.Extensions.Options;

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
            //启动服务
            server.Start();
            var ipAndPort = server.Ports.FirstOrDefault();
            if (ipAndPort == null) return server;
            //输出启动信息
            MetaModel.StartTime = DateTime.Now;
            MetaModel.Ip = ipAndPort.Host;
            MetaModel.Port = ipAndPort.BoundPort;
            Console.WriteLine($"server listening {MetaModel.Ip}:{MetaModel.Port}");

            //服务注册
            var grpcServerOptions = ServiceProviderAccessor.GetService<IOptions<GrpcServerOptions>>().Value;
            var registerIP = NetHelper.GetIp(grpcServerOptions.ServiceAddress);
            
            if (grpcServerOptions.EnableDiscovery)
            {
                //检查服务注册配制
                if (string.IsNullOrWhiteSpace(grpcServerOptions.DiscoveryUrl))
                    throw new ArgumentException("GrpcServer:DiscoveryUrl is null");
                if (string.IsNullOrWhiteSpace(grpcServerOptions.DiscoveryServiceName))
                    throw new ArgumentException("GrpcServer:DiscoveryServiceName is null");

                //服务注册
                var serviceRegister = ServiceProviderAccessor.GetService<IServiceRegister>();
                Console.WriteLine($"use {serviceRegister.GetType().Name} register");
                Console.WriteLine($"    DiscoveryUrl:{grpcServerOptions.DiscoveryUrl}");
                Console.WriteLine($"    ServiceName:{grpcServerOptions.DiscoveryServiceName}");
                var registerModel = grpcServerOptions.ToJson().FromJson<ServiceRegisterModel>();
                registerModel.ServiceIp = registerIP;
                registerModel.ServicePort = ipAndPort.BoundPort;
                serviceRegister.RegisterService(registerModel).Wait();
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
            var grpcServerOptions = ServiceProviderAccessor.GetService<IOptions<GrpcServerOptions>>().Value;
            if (grpcServerOptions.EnableDiscovery)
            {
                var serviceRegister = ServiceProviderAccessor.GetService<IServiceRegister>();
                serviceRegister.DeregisterService().Wait();
            }
            //停止服务
            server.ShutdownAsync().Wait();
            
            return server;
        }
        #endregion
    }
}
