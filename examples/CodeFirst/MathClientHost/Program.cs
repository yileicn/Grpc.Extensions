using Grpc.Extension.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using static MathGrpc.MathGrpc;
using Grpc.Extension.Abstract.Model;

namespace MathClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //使用配制文件
            var configPath = Path.Combine(AppContext.BaseDirectory, "config");
            var host = new HostBuilder()
                .ConfigureAppConfiguration((ctx, conf) =>
                {
                    conf.SetBasePath(configPath);
                    conf.AddJsonFile("appsettings.json", false, true);
                })
                .ConfigureServices((ctx, services)=> {
                    services.AddGrpcClientExtensions(ctx.Configuration);//注入GrpcClientExtensions
                    services.AddGrpcClient<MathGrpcClient>("Math.Test");//注入grpc client
                })
                .Build();
               
            var provider = host.Services;
            //配制GrpcClientApp
            var clientApp = provider.GetService<GrpcClientApp>();
            clientApp.
                //使用日志(默认使用LoggerFactory)
                UseLogger((log) =>
                {
                    log.LoggerMonitor += (msg, type) => Console.WriteLine(GetLogTypeName(type) + ":" + msg);
                    log.LoggerError += (ex, type) => Console.WriteLine(GetLogTypeName(type) + ":" + ex);
                }).Run();

            //从容器获取client
            var client = provider.GetService<MathGrpcClient>();

            Console.WriteLine("start:");
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    var reply = client.Add(new MathGrpc.AddRequest() { Num1 = 1, Num2 = i});
                    Console.WriteLine($"Add {i.ToString()}: {reply.Value}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static string GetLogTypeName(LogType logtype)
        {
            return Enum.GetName(typeof(LogType), logtype);
        }
    }
}
