using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Grpc.Extension.Interceptors
{
    public class ClientInterceptor : Interceptor
    {
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, Interceptor.BlockingUnaryCallContinuation<TRequest, TResponse> continuation) 
        {
            return continuation(request, context);
        }
      
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, Interceptor.AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            return continuation(request, context);
        }
    }
}
