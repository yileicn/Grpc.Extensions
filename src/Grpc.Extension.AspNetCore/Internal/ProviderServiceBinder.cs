using Grpc.AspNetCore.Server.Model;
using Grpc.Core;
using Grpc.Extension.Abstract;
using Grpc.Extension.BaseService;
using Grpc.Extension.BaseService.Model;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Grpc.Extension.AspNetCore.Internal
{
    internal class ProviderServiceBinder<TService> : ServiceBinderBase where TService : class
    {
        private readonly ServiceMethodProviderContext<TService> _context;
        private readonly Type _declaringType;
        private readonly bool _isIGrpcService;
        private readonly bool _isIGrpcBaseService;

        internal ProviderServiceBinder(ServiceMethodProviderContext<TService> context, Type declaringType)
        {
            _context = context;
            _declaringType = declaringType;
            _isIGrpcService = typeof(IGrpcService).IsAssignableFrom(typeof(TService));
            _isIGrpcBaseService = typeof(IGrpcBaseService).IsAssignableFrom(typeof(TService));
        }

        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ClientStreamingServerMethod<TRequest, TResponse> handler)
        {
            var (invoker, metadata) = CreateModelCore<ClientStreamingServerMethod<TService, TRequest, TResponse>>(
                method.Name,
                new[] { typeof(IAsyncStreamReader<TRequest>), typeof(ServerCallContext) });
            if (_isIGrpcService)
            {
                _context.AddClientStreamingMethod<TRequest, TResponse>(method, metadata, invoker);
            }
            AddMetaMethod((new MetaMethodModel
            {
                FullName = method.FullName,
                RequestType = typeof(TRequest),
                ResponseType = typeof(TResponse),
                ServiceType = typeof(TService),
                Handler = invoker
            })); 
        }

        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, DuplexStreamingServerMethod<TRequest, TResponse> handler)
        {
            var (invoker, metadata) = CreateModelCore<DuplexStreamingServerMethod<TService, TRequest, TResponse>>(
                method.Name,
                new[] { typeof(IAsyncStreamReader<TRequest>), typeof(IServerStreamWriter<TResponse>), typeof(ServerCallContext) });
            if (_isIGrpcService)
            {
                _context.AddDuplexStreamingMethod<TRequest, TResponse>(method, metadata, invoker);
            }
            AddMetaMethod((new MetaMethodModel
            {
                FullName = method.FullName,
                RequestType = typeof(TRequest),
                ResponseType = typeof(TResponse),
                ServiceType = typeof(TService),
                Handler = invoker
            }));
        }

        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, ServerStreamingServerMethod<TRequest, TResponse> handler)
        {
            var (invoker, metadata) = CreateModelCore<ServerStreamingServerMethod<TService, TRequest, TResponse>>(
                method.Name,
                new[] { typeof(TRequest), typeof(IServerStreamWriter<TResponse>), typeof(ServerCallContext) });
            if (_isIGrpcService)
            {
                _context.AddServerStreamingMethod<TRequest, TResponse>(method, metadata, invoker);
            }
            AddMetaMethod((new MetaMethodModel
            {
                FullName = method.FullName,
                RequestType = typeof(TRequest),
                ResponseType = typeof(TResponse),
                ServiceType = typeof(TService),
                Handler = invoker
            }));
        }

        public override void AddMethod<TRequest, TResponse>(Method<TRequest, TResponse> method, UnaryServerMethod<TRequest, TResponse> handler)
        {
            var (invoker, metadata) = CreateModelCore<UnaryServerMethod<TService, TRequest, TResponse>>(
                method.Name,
                new[] { typeof(TRequest), typeof(ServerCallContext) });
            if (_isIGrpcService)
            {
                _context.AddUnaryMethod<TRequest, TResponse>(method, metadata, invoker);
            }
            AddMetaMethod((new MetaMethodModel
            {
                FullName = method.FullName,
                RequestType = typeof(TRequest),
                ResponseType = typeof(TResponse),
                ServiceType =  typeof(TService),
                Handler = invoker
            }));
        }

        private Delegate GetOrCreateHandler<TDelegate>(Delegate handler ,string methodName, Type[] methodParameters)
        {
            if (handler != null) return handler;
            //创建Handler
            var handlerMethod = GetMethod(methodName, methodParameters);
            if (handlerMethod == null)
            {
                throw new InvalidOperationException($"Could not find '{methodName}' on {typeof(TService)}.");
            }
            handler = handlerMethod.CreateDelegate(typeof(TDelegate),null);

            return handler;
        }

        private (TDelegate invoker, List<object> metadata) CreateModelCore<TDelegate>(string methodName, Type[] methodParameters) where TDelegate : Delegate
        {
            var handlerMethod = GetMethod(methodName, methodParameters);

            if (handlerMethod == null)
            {
                throw new InvalidOperationException($"Could not find '{methodName}' on {typeof(TService)}.");
            }

            var invoker = (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), handlerMethod);

            var metadata = new List<object>();
            // Add type metadata first so it has a lower priority
            metadata.AddRange(typeof(TService).GetCustomAttributes(inherit: true));
            // Add method metadata last so it has a higher priority
            metadata.AddRange(handlerMethod.GetCustomAttributes(inherit: true));

            // Accepting CORS preflight means gRPC will allow requests with OPTIONS + preflight headers.
            // If CORS middleware hasn't been configured then the request will reach gRPC handler.
            // gRPC will return 405 response and log that CORS has not been configured.
            metadata.Add(new HttpMethodMetadata(new[] { "POST" }, acceptCorsPreflight: true));

            return (invoker, metadata);
        }

        private MethodInfo? GetMethod(string methodName, Type[] methodParameters)
        {
            Type? currentType = typeof(TService);
            while (currentType != null)
            {
                var matchingMethod = currentType.GetMethod(
                    methodName,
                    BindingFlags.Public | BindingFlags.Instance,
                    binder: null,
                    types: methodParameters,
                    modifiers: null);

                if (matchingMethod == null)
                {
                    return null;
                }
                if (_isIGrpcService)
                {
                    return matchingMethod;
                }
                // Validate that the method overrides the virtual method on the base service type.
                // If there is a method with the same name it will hide the base method. Ignore it,
                // and continue searching on the base type.
                if (matchingMethod.IsVirtual)
                {
                    var baseDefinitionMethod = matchingMethod.GetBaseDefinition();
                    if (baseDefinitionMethod != null && baseDefinitionMethod.DeclaringType == _declaringType)
                    {
                        return matchingMethod;
                    }
                }

                currentType = currentType.BaseType;
            }

            return null;
        }

        private void AddMetaMethod(MetaMethodModel model)
        {
            if (_isIGrpcBaseService) return;

            MetaModel.Methods.Add(model);
        }
    }
}
