using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Extension;
using Grpc.Extension.BaseService;
using Grpc.Extension.Interceptors;
using Grpc.Extension.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace MathServer
{
    public class GrpcHostServiceV2 : IHostedService
    {
        private Server _server;
        private IConfiguration _conf;
        private ServerBuilder _serverBuilder;

        public GrpcHostServiceV2(IConfiguration conf,
            ServerBuilder serverBuilder)
        {
            this._conf = conf;
            this._serverBuilder = serverBuilder;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //构建Server
            _server = _serverBuilder
                .UseOptions(options => {
                    //options.GlobalPackage = "MathGrpc";
                    options.ProtoNameSpace = "MathGrpc";
                })
                .UseDashBoard()//使用DashBoard,需要使用FM.GrpcDashboard网站
                .UseLogger(log =>//使用日志
                {
                    log.LoggerMonitor = info => Console.WriteLine(info);
                    log.LoggerError = exception => Console.WriteLine(exception);
                })
                .UseProtoGenerate("proto")//生成proto
                .Build();
            //启动服务并注册到consul
            _server.StartAndRegisterService();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _server?.StopAndDeRegisterService();//停止服务并从consul反注册
            return Task.CompletedTask;
        }
    }
}
