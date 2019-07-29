using Grpc.Core;
using Grpc.Core.Interceptors;
using OpenTracing.Util;
using System;
using Grpc.Extension.Common;
using System.Linq;
using OpenTracing;

namespace Grpc.Extension.Interceptors
{
    internal class ClientJaegerTracingInterceptor : ClientInterceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
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
            var header = context.Options.Headers?.Where(p => p.Key == JaegerTracingInterceptor.jaegerKey).FirstOrDefault();
            if (header == null)
            {
                var metaEntry = new Metadata.Entry(JaegerTracingInterceptor.jaegerKey, span.Context.ToString());
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
