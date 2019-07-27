using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Grpc.Extension.Interceptors
{
    /// <summary>
    /// 客户端拦截器
    /// </summary>
    public class ClientInterceptor : Interceptor
    {
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
            return continuation(request, context);
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
            return continuation(request, context);
        }

        /// <summary>
        /// 异步客户端流请求
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            return continuation(context);
        }

        /// <summary>
        /// 异步服务端流返回
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            return continuation(request, context);
        }

        /// <summary>
        /// 异步双向流
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="context"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            return continuation(context);
        }
   
    }
}
