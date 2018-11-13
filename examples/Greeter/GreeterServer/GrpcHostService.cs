using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Extension;
using Grpc.Extension.Model;
using Helloworld;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GreeterServer
{
    public class GrpcHostService : IHostedService
    {
        private Server _server;
        private IConfiguration _conf;

        public GrpcHostService(IConfiguration conf)
        {
            this._conf = conf;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var grpcService = Greeter.BindService(new GreeterImpl())
                .UseBaseInterceptor()//使用基本的过滤器(性能监控,熔断处理)
                .UseDashBoard()//使用DashBoard,需要使用FM.GrpcDashboard
                .UseLogger(log =>//使用日志
                {
                    log.LoggerMonitor = info => Console.WriteLine(info);
                    log.LoggerError = exception => Console.WriteLine(exception);
                });
            _server = new Server
            {
                Services = { grpcService },
                Ports = { new ServerPort(_conf["ServiceAddress"], 0, ServerCredentials.Insecure) }
            };
            _server.UseConsulConfig(_conf["ConsulUrl"], _conf["ConsulServiceName"])//使用Consul
                .StartAndRegisterService();//启动服务并注册到consul

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _server?.StopAndDeRegisterService();//停止服务并从consul反注册
            return Task.CompletedTask;
        }
    }
}
