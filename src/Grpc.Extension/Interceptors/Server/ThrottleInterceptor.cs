using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Extension.Internal;

namespace Grpc.Extension.Interceptors
{
    /// <summary>
    /// 手动熔断处理
    /// </summary>
    public class ThrottleInterceptor : ServerInterceptor
    {
        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
            ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            if (ThrottleManager.Instance.IsThrottled(context.Method))
            {
                throw new RpcException(new Status(
                    StatusCode.Cancelled,
                    Newtonsoft.Json.JsonConvert.SerializeObject(new { Code = 503, Detail = Consts.ThrottledMsg })));
            }
            return continuation(request, context);
        }
    }
}
