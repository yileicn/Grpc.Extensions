using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using Grpc.Extension.Consul;
using Grpc.Extension.Model;
using System.Reflection;
using Grpc.Extension.Common;

namespace Grpc.Extension
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 根据consul自动生成channel
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection UseAutoGrpcChannel(this IServiceCollection services)
        {
            services.AddSingleton<CallInvoker, AutoChannelCallInvoker>();
            return services;
        }

        public static IServiceCollection AddGrpcClient<T>(this IServiceCollection services, string consulUrl,string consulServiceName) where T: class
        {
            services.AddSingleton<T>();
            var channelConfig = new ChannelConfig
            {
                ConsulUrl = consulUrl,
                ConsulServiceName = consulServiceName
            };
            var bindFlags = BindingFlags.Static | BindingFlags.NonPublic;
            channelConfig.GrpcServiceName = typeof(T).DeclaringType.GetFieldValue<string>("__ServiceName", bindFlags);
            ChannelManager.Instance.Configs.Add(channelConfig);
            return services;
        }
    }
}
