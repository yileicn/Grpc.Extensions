using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Extension.Consul;
using Grpc.Extension.Interceptors;
using Grpc.Extension.Internal;

namespace Grpc.Extension
{
    /// <summary>
    /// 客户端中间件的CallInvoker
    /// </summary>
    public class InterceptorCallInvoker : CallInvoker
    {
        private CallInvoker _interceptorCallInvoker;
        public InterceptorCallInvoker(AutoChannelCallInvoker autoChannelCallInvoker, IEnumerable<ClientInterceptor> clientInterceptors)
        {
            _interceptorCallInvoker = autoChannelCallInvoker.Intercept(clientInterceptors.ToArray());
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return _interceptorCallInvoker.BlockingUnaryCall(method, host, options, request);
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return _interceptorCallInvoker.AsyncUnaryCall(method, host, options, request);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options, TRequest request)
        {
            return _interceptorCallInvoker.AsyncServerStreamingCall(method, host, options, request);
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            return _interceptorCallInvoker.AsyncClientStreamingCall(method, host, options);
        }

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(Method<TRequest, TResponse> method, string host, CallOptions options)
        {
            return _interceptorCallInvoker.AsyncDuplexStreamingCall(method, host, options);
        }
    }
}
