using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Extension.Common;
using OpenTracing.Util;
using System;
using System.Threading.Tasks;
using System.Linq;
using Jaeger;
using OpenTracing;
using Grpc.Extension.Client.Interceptors;
using Grpc.Extension.Interceptors;

namespace MathServer.Middlewares
{
    /// <summary>
    /// JaegerTracingMiddleware
    /// </summary>
    public class JaegerTracingMiddleware : ServerInterceptor
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

    public class ClientJaegerTracingMiddleware : ClientInterceptor
    {
        public override  AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var tracer = GlobalTracer.Instance;
            var method = $"{context.Method.ServiceName}/{context.Method.Name}";
            var span = tracer.BuildSpan(method).AsChildOf(tracer.ActiveSpan).WithTag("Request",request?.ToJson() ?? "").Start();
            context = SetJaegerHeader(context, span);
            try
            {
                return continuation(request, context);
            }
            catch (Exception ex)
            {
                span.SetTag("Error", ex.ToString());
                throw;
            }
            finally
            {
                span.Finish();
            }
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var tracer = GlobalTracer.Instance;
            var method = $"{context.Method.ServiceName}/{context.Method.Name}";
            var span = tracer.BuildSpan(method).AsChildOf(tracer.ActiveSpan).WithTag("Request", request?.ToJson() ?? "").Start();
            context = SetJaegerHeader(context, span);
            try
            {
                return continuation(request, context);
            }
            catch (Exception ex)
            {
                span.SetTag("Error", ex.ToString());
                throw;
            }
            finally
            {
                span.Finish();
            }
        }

        private ClientInterceptorContext<TRequest, TResponse> SetJaegerHeader<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, ISpan span)
            where TRequest : class
            where TResponse : class
        {
            var header = context.Options.Headers?.Where(p => p.Key == JaegerTracingMiddleware.jaegerKey).FirstOrDefault();
            if (header == null)
            {
                var metaEntry = new Metadata.Entry("jaeger", span.Context.ToString());
                if (context.Options.Headers == null)
                {
                    var meta = new Metadata();
                    meta.Add(metaEntry);
                    var callOptions = context.Options.WithHeaders(meta);
                    context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, callOptions);
                }
                else
                {
                    context.Options.Headers.Add(metaEntry);
                }
            }
            return context;
        }
    }
}
