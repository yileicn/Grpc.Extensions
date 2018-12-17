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
using Grpc.Core;
using Grpc.Extension;
using Grpc.Extension.Interceptors;
using Grpc.Extension.Model;
using Helloworld;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GreeterClient
{
    class Program
    {
        public static void Main(string[] args)
        {
            //使用配制文件
            var configPath = Path.Combine(AppContext.BaseDirectory, "config");
            var configBuilder = new ConfigurationBuilder();
            var config = configBuilder.SetBasePath(configPath).AddJsonFile("appsettings.json", false, true).Build();
            //使用依赖注入
            var services = new ServiceCollection()
                .AddGrpcExtensions()//注入GrpcExtensions
                .AddSingleton<ClientInterceptor>(new ClientCallTimeout(10))//注入客户端中间件
                .AddGrpcClient<Greeter.GreeterClient>(config["ConsulUrl"], "Greeter.Test");//注入grpc client
            var provider = services.BuildServiceProvider();
            
            //从容器获取client
            var client = provider.GetService<Greeter.GreeterClient>();
            //StreamTest(client).Wait();

            var user = "you";

            for (int i = 0; i < 10; i++)
            {
                var reply = client.SayHello(new HelloRequest { Name = user + i.ToString() });
                Console.WriteLine($"Greeting{i.ToString()}: {reply.Message}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
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
