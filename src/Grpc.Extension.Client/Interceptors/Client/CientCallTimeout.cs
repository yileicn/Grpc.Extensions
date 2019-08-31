using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Grpc.Extension.Client.Interceptors
{
    /// <summary>
    /// 客户端超时拦截器
    /// </summary>
    internal class ClientCallTimeout : ClientInterceptor
    {
        private double _callTimeOutSecond;

        /// <summary>
        /// 客户端超时拦截器
        /// </summary>
        /// <param name="callTimeOutSecond"></param>
        public ClientCallTimeout(double callTimeOutSecond)
        {
            this._callTimeOutSecond = callTimeOutSecond;
        }

        /// <summary>
        /// 同步调用
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, Interceptor.BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var callOptions = SetDeadline(context.Options);
            var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, callOptions);
            return continuation(request, newContext);
        }
      
        /// <summary>
        /// 异步调用
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, Interceptor.AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            var callOptions = SetDeadline(context.Options);
            var newContext = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, callOptions);
            return continuation(request, newContext);
        }

        private CallOptions SetDeadline(CallOptions callOptions)
        {
            if (callOptions.Deadline == null)
            {
                callOptions = callOptions.WithDeadline(DateTime.UtcNow.AddSeconds(_callTimeOutSecond));
            }
            return callOptions;
        }
    }
}
