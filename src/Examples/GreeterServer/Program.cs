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
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Extension;
using Helloworld;

namespace GreeterServer
{
    class GreeterImpl : Greeter.GreeterBase
    {
        // Server side handler of the SayHello RPC
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
        }
    }

    class Program
    {
        const int Port = 50051;

        public static void Main(string[] args)
        {
            var grpcService = Greeter.BindService(new GreeterImpl())
                .UseBaseInterceptor()//使用基本的过滤器(性能监控,熔断处理)
                .UseDashBoard()//使用DashBoard,需要使用FM.GrpcDashboard
                .UseLogger(log =>//使用日志
                {
                    log.LoggerMonitor = info => Console.WriteLine(info);
                    log.LoggerError = exception => Console.WriteLine(exception);
                });
            Server server = new Server
            {
                Services = { grpcService },
                Ports = { new ServerPort("192.168.0.137", Port, ServerCredentials.Insecure) }
            };
            server.UseConsulConfig("http://192.168.8.6:8500","test")//使用Consul
                .StartAndRegisterService();//启动服务并注册到consul

            Console.WriteLine("Greeter server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.StopAndDeRegisterService();//停止服务并从consul反注册
        }
    }
}
