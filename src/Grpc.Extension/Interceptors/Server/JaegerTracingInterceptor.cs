using Grpc.Core;
using OpenTracing.Util;
using System;
using System.Threading.Tasks;
using System.Linq;
using Jaeger;
using Grpc.Extension.Common;

namespace Grpc.Extension.Interceptors
{
    /// <summary>
    /// JaegerTracingMiddleware
    /// </summary>
    internal class JaegerTracingInterceptor : ServerInterceptor
    {
        public const string jaegerKey = "jaeger";
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var header = context.RequestHeaders.Where(p => p.Key == jaegerKey).FirstOrDefault();
            var spanBuilder = GlobalTracer.Instance.BuildSpan(context.Method).WithTag("Request", request?.ToJson() ?? "");
            if(header != null)
            {
                var spanContext = SpanContext.ContextFromString(header.Value);
                spanBuilder = spanBuilder.AsChildOf(spanContext);
            }
            using (var scope = spanBuilder.StartActive(true))
            {
                try
                {
                    return await continuation(request, context);
                }
                catch (Exception ex)
                {
                    scope.Span.SetTag("Error", ex.ToString());
                    throw;
                }
            }
        }
    }
}
