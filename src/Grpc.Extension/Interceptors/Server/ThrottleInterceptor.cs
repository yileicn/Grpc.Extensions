using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Extension.Abstract;
using Grpc.Extension.Internal;

namespace Grpc.Extension.Interceptors
{
    /// <summary>
    /// 手动熔断处理
    /// </summary>
    internal class ThrottleInterceptor : ServerInterceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
            ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            if (ThrottleManager.Instance.IsThrottled(context.Method))
            {
                throw new RpcException(new Status(
                    StatusCode.Cancelled,
                    Newtonsoft.Json.JsonConvert.SerializeObject(new { Code = 503, Detail = ServerConsts.ThrottledMsg })));
            }
            return await continuation(request, context);
        }

        public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream,
            ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            if (ThrottleManager.Instance.IsThrottled(context.Method))
            {
                throw new RpcException(new Status(
                    StatusCode.Cancelled,
                    Newtonsoft.Json.JsonConvert.SerializeObject(new { Code = 503, Detail = ServerConsts.ThrottledMsg })));
            }
            return await continuation(requestStream, context);
        }

        public override async Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            if (ThrottleManager.Instance.IsThrottled(context.Method))
            {
                throw new RpcException(new Status(
                    StatusCode.Cancelled,
                    Newtonsoft.Json.JsonConvert.SerializeObject(new { Code = 503, Detail = ServerConsts.ThrottledMsg })));
            }
            await continuation(request, responseStream, context);
        }

        public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            if (ThrottleManager.Instance.IsThrottled(context.Method))
            {
                throw new RpcException(new Status(
                    StatusCode.Cancelled,
                    Newtonsoft.Json.JsonConvert.SerializeObject(new { Code = 503, Detail = ServerConsts.ThrottledMsg })));
            }
            await continuation(requestStream, responseStream, context);
        }
    }
}
