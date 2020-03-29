using Grpc.Core;
using Grpc.Extension.BaseService.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.Internal
{
    internal class GrpcServiceBinder : ServiceBinderBase
    {
        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, UnaryServerMethod<TRequest, TResponse> handler)
        {
            MetaModel.Methods.Add((new MetaMethodModel
            {
                FullName = method.FullName,
                RequestType = typeof(TRequest),
                ResponseType = typeof(TResponse),
                Handler = handler
            }));
        }

        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ClientStreamingServerMethod<TRequest, TResponse> handler)
        {
            MetaModel.Methods.Add((new MetaMethodModel
            {
                FullName = method.FullName,
                RequestType = typeof(TRequest),
                ResponseType = typeof(TResponse),
                Handler = handler
            }));
        }

        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServerStreamingServerMethod<TRequest, TResponse> handler)
        {
            MetaModel.Methods.Add((new MetaMethodModel
            {
                FullName = method.FullName,
                RequestType = typeof(TRequest),
                ResponseType = typeof(TResponse),
                Handler = handler
            }));
        }

        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, DuplexStreamingServerMethod<TRequest, TResponse> handler)
        {
            MetaModel.Methods.Add((new MetaMethodModel
            {
                FullName = method.FullName,
                RequestType = typeof(TRequest),
                ResponseType = typeof(TResponse),
                Handler = handler
            }));
        }
    }
}
