using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Grpc.Extension.Interceptors
{
    public class ClientCallTimeout : ClientInterceptor
    {
        private int _callTimeOut;
        public ClientCallTimeout(int callTimeOut)
        {
            this._callTimeOut = callTimeOut;
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, Interceptor.BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var callOptions = SetDeadline(context.Options);
            var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, callOptions);
            return continuation(request, newContext);
        }
      
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, Interceptor.AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var callOptions = SetDeadline(context.Options);
            var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, callOptions);
            return continuation(request, context);
        }

        private CallOptions SetDeadline(CallOptions callOptions)
        {
            if (callOptions.Deadline == null)
            {
                callOptions = callOptions.WithDeadline(DateTime.UtcNow.AddSeconds(_callTimeOut));
            }
            return callOptions;
        }
    }
}
