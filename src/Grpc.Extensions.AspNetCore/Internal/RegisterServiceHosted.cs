using Grpc.Extension.Abstract.Discovery;
using Grpc.Extension.BaseService.Model;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Extension.AspNetCore;
using Microsoft.Extensions.Options;
using Grpc.Extension.Abstract.Model;
using Grpc.Extension.Common;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Linq;

namespace Grpc.Extensions.AspNetCore.Internal
{
    public class RegisterServiceHosted : IHostedService
    {
        private readonly IServiceRegister _serviceRegister;
        private readonly IServer _server;
        private readonly GrpcServerOptions _grpcServerOptions;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger _logger;

        public RegisterServiceHosted(IServiceRegister serviceRegister, IServer server, IOptions<GrpcServerOptions> grpcServerOptions,
            IHostApplicationLifetime hostApplicationLifetime, ILogger<RegisterServiceHosted> logger)
        {
            _serviceRegister = serviceRegister;
            _server = server;
            _grpcServerOptions = grpcServerOptions.Value;
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = logger;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _hostApplicationLifetime.ApplicationStarted.Register(Start);
            return Task.CompletedTask;
        }

        public void Start()
        {
            var serverAddressesFeature = _server.Features?.Get<IServerAddressesFeature>();
            if (serverAddressesFeature == null || serverAddressesFeature.Addresses.Count == 0)
            {
                _logger.LogError("can not found IServerAddressesFeature in IServer,can not register service.");
                return;
            }
            //添加服务IPAndPort
            var ipPort = NetHelper.GetIPAndPort(_grpcServerOptions.ServiceAddress);
            var serverAddress = BindingAddress.Parse(serverAddressesFeature.Addresses.First());

            MetaModel.StartTime = DateTime.Now;
            MetaModel.Ip = ipPort.Item1;
            MetaModel.Port = serverAddress.Port;
            Console.WriteLine($"server listening {MetaModel.Ip}:{MetaModel.Port}");
            //使用BaseServices
            Console.WriteLine($"use {_serviceRegister.GetType().Name} register");
            Console.WriteLine($"    DiscoveryUrl:{_grpcServerOptions.DiscoveryUrl}");
            Console.WriteLine($"    ServiceName:{_grpcServerOptions.DiscoveryServiceName}");
            var registerModel = _grpcServerOptions.ToJson().FromJson<ServiceRegisterModel>();
            registerModel.ServiceIp = MetaModel.Ip;
            registerModel.ServicePort = MetaModel.Port;
            _serviceRegister.RegisterService(registerModel);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _serviceRegister.DeregisterService();

            return Task.CompletedTask;
        }
    }
}
