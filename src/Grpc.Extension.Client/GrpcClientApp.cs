using Grpc.Extension.Abstract;
using Microsoft.Extensions.Configuration;
using System;

namespace Grpc.Extension.Client
{
    /// <summary>
    /// GrpcClient启动类
    /// </summary>
    public class GrpcClientApp
    {
        private IConfiguration _conf;

        /// <summary>
        /// GrpcClientApp
        /// </summary>
        /// <param name="conf"></param>
        public GrpcClientApp(IConfiguration conf)
        {
            _conf = conf;

            //初始化配制
            this.InitGrpcOption();
        }

        /// <summary>
        /// 从配制文件初始化
        /// </summary>
        /// <returns></returns>
        private void InitGrpcOption()
        {
            //初始化GrpcClientOption
            var clientConfig = _conf.GetSection("GrpcClient").Get<GrpcClientOptions>();

            if(clientConfig != null)
            {
                var clientOptions = GrpcClientOptions.Instance;
                clientOptions.DiscoveryUrl = clientConfig.DiscoveryUrl;
                clientOptions.ServiceAddressCacheTime = clientConfig.ServiceAddressCacheTime;
                clientOptions.DefaultErrorCode = clientConfig.DefaultErrorCode;
            }
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
        /// 配制日志
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public GrpcClientApp UseLogger(Action<LoggerAccessor> action)
        {
            action(LoggerAccessor.Instance);
            return this;
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
