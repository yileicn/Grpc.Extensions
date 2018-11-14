using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Extension;
using Grpc.Extension.BaseService;
using Grpc.Extension.Internal;
using Grpc.Extension.Model;
using Helloworld;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GreeterServer
{
    public class GrpcHostServiceV2 : IHostedService
    {
        private Server _server;
        private IConfiguration _conf;

        public GrpcHostServiceV2(IConfiguration conf)
        {
            this._conf = conf;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //构建Server
            var serverBuilder = new ServerBuilder();
            var serverOptions = _conf.GetSection("GrpcServer").Get<GrpcServerOptions>();
            _server = serverBuilder.UseGrpcOptions(serverOptions)
                .UseBaseInterceptor() //使用基本的过滤器(性能监控,熔断处理
                .UseGrpcService(Greeter.BindService(new GreeterImpl()))
                .Build();
            //使用DashBoard，日志，注册consul
            _server.UseDashBoard()//使用DashBoard,需要使用FM.GrpcDashboard网站
                .UseLogger(log =>//使用日志
                {
                    log.LoggerMonitor = info => Console.WriteLine(info);
                    log.LoggerError = exception => Console.WriteLine(exception);
                })
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
