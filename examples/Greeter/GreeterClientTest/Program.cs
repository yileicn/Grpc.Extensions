// Copyright 2015 gRPC authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Threading.Tasks;
using Grpc.Extension.Abstract.Model;
using Grpc.Extension.Client;
using Helloworld;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreeterClient
{
    class Program
    {
        public static void Main(string[] args)
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
                    services.AddGrpcClient<Greeter.GreeterClient>("Greeter.Test");//注入grpc client
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
            var client = provider.GetService<Greeter.GreeterClient>();
            //StreamTest(client).Wait();

            var user = "you";

            try
            {
                for (int i = 0; i < 10; i++)
                {
                    var reply = client.SayHello(new HelloRequest { Name = user + i.ToString() });
                    Console.WriteLine($"Greeting{i.ToString()}: {reply.Message}");
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

        public static async Task StreamTest(Greeter.GreeterClient client)
        {
            using (var stream = client.SayHelloStream())
            {
                await stream.RequestStream.WriteAsync(new HelloRequest() { Name = "yilei" });
                await stream.RequestStream.WriteAsync(new HelloRequest() { Name = "zhangshan" });
                await stream.RequestStream.CompleteAsync();
                Console.WriteLine("GreetingStream:" + stream.ResponseAsync.Result.Message);
            }
                
        }
    }
}
