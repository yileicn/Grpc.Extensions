using Grpc.Extension.Abstract;
using Grpc.Extension.Common.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;
using System;

namespace Grpc.Extension.Client
{
    /// <summary>
    /// GrpcClient启动类
    /// </summary>
    public class GrpcClientApp
    {
        private IConfiguration _conf;
        private readonly ILoggerFactory _loggerFactory;

        /// <summary>
        /// GrpcClientApp
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="conf"></param>
        /// <param name="loggerFactory"></param>
        public GrpcClientApp(IServiceProvider serviceProvider, IConfiguration conf, ILoggerFactory loggerFactory)
        {
            ServiceProviderAccessor.SetServiceProvider(serviceProvider);
            _conf = conf;
            _loggerFactory = loggerFactory;

            this.InitGrpcOption()//初始化配制
                .UseLoggerFactory()//使用LoggerFactory
                .UseJaeger();

        }

        /// <summary>
        /// 从配制文件初始化
        /// </summary>
        /// <returns></returns>
        private GrpcClientApp InitGrpcOption()
        {
            //初始化GrpcClientOption
            var clientConfig = _conf.GetSection("GrpcClient").Get<GrpcClientOptions>();

            if(clientConfig != null)
            {
                var clientOptions = GrpcClientOptions.Instance;
                clientOptions.DiscoveryUrl = clientConfig.DiscoveryUrl;
                clientOptions.ServiceAddressCacheTime = clientConfig.ServiceAddressCacheTime;
                clientOptions.DefaultErrorCode = clientConfig.DefaultErrorCode;
                clientOptions.Jaeger = clientConfig.Jaeger;
                clientOptions.GrpcCallTimeOut = clientConfig.GrpcCallTimeOut;
            }

            return this;
        }

        /// <summary>
        /// 注入Grpc,Discovery配制
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public GrpcClientApp UseGrpcOptions(Action<GrpcClientOptions> options)
        {
            options(GrpcClientOptions.Instance);
            return this;
        }

        /// <summary>
        /// 使用LoggerFactory
        /// </summary>
        /// <returns></returns>
        private GrpcClientApp UseLoggerFactory()
        {
            var _logger = _loggerFactory.CreateLogger<GrpcClientApp>();
            var _loggerAccess = _loggerFactory.CreateLogger("grpc.access");

            LoggerAccessor.Instance.LoggerError += (ex, type) => _logger.LogError(ex.ToString());
            LoggerAccessor.Instance.LoggerMonitor += (msg, type) => _loggerAccess.LogInformation(msg);

            return this;
        }

        /// <summary>
        /// 配制日志(默认使用LoggerFactory)
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public GrpcClientApp UseLogger(Action<LoggerAccessor> action)
        {
            action(LoggerAccessor.Instance);
            return this;
        }

        /// <summary>
        /// 有Jaeger配制就使用Jaeger
        /// </summary>
        private void UseJaeger()
        {
            var jaeger = GrpcClientOptions.Instance.Jaeger;
            if (jaeger?.CheckConfig() == true)
            {
                var tracer = ServiceProviderAccessor.GetService<ITracer>();
                if (tracer != null) GlobalTracer.Register(tracer);
            }
        }

        /// <summary>
        /// 启动
        /// </summary>
        public void Run()
        {
            //检查服务发现配制
            if (string.IsNullOrWhiteSpace(GrpcClientOptions.Instance.DiscoveryUrl))
                throw new ArgumentException("GrpcClient:DiscoveryUrl is null");
        }
    }
}
