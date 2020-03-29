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
using GreeterServer.Common;
using Grpc.Extension.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreeterServer
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (var host = BuildHost(args))
            {
                host.Run();
            }
        }

        public static IHost BuildHost(string[] args)
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "config");
            var host = new HostBuilder()
                .ConfigureHostConfiguration(conf =>
                {
                    conf.SetBasePath(configPath);
                    conf.AddJsonFile("hostsettings.json", optional: true);
                })
                .ConfigureAppConfiguration((ctx, conf) =>
                {
                    conf.SetBasePath(configPath);
                    conf.AddJsonFile("appsettings.json", false, true);
                })
                .UseStartup<Startup>();
            return host.Build();
        }
    }
}
