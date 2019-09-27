using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Extension;
using Grpc.Extension.Abstract.Model;
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
                    options.GlobalPackage = "math";
                    options.ProtoNameSpace = "MathGrpc";
                })
                .UseGrpcService()
                .UseDashBoard()//使用DashBoard,需要使用FM.GrpcDashboard网站
                //.UseLogger(log =>//使用日志(默认使用LoggerFactory,手动覆盖)
                //{
                //    log.LoggerMonitor = (msg, type) => Console.WriteLine(GetLogTypeName(type) + ":" + msg);
                //    log.LoggerError = (ex, type) => Console.WriteLine(GetLogTypeName(type) + ":" + ex);
                //})
                .UseProtoGenerate("proto")//生成proto
                .Build();
            //启动服务并注册到consul
            _server.StartAndRegisterService();

            return Task.CompletedTask;
        }

        private static string GetLogTypeName(LogType logtype)
        {
            return Enum.GetName(typeof(LogType), logtype);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _server?.StopAndDeRegisterService();//停止服务并从consul反注册
            return Task.CompletedTask;
        }
    }
}
