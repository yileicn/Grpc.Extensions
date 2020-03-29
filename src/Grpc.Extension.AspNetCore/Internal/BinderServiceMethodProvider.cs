using Grpc.AspNetCore.Server.Model;
using Grpc.Extension.Abstract;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Grpc.Extension.AspNetCore.Internal
{
    internal class BinderServiceMethodProvider<TService> : IServiceMethodProvider<TService> where TService : class
    {
        public void OnServiceMethodDiscovery(ServiceMethodProviderContext<TService> context)
        {
            var bindMethodInfo = BindMethodFinder.GetBindMethod(typeof(TService));

            // Invoke BindService(ServiceBinderBase, BaseType)
            if (bindMethodInfo != null)
            {
                // The second parameter is always the service base type
                var serviceParameter = bindMethodInfo.GetParameters()[1];

                var binder = new ProviderServiceBinder<TService>(context, serviceParameter.ParameterType);

                try
                {
                    if (typeof(IGrpcService).IsAssignableFrom(typeof(TService)))
                        bindMethodInfo.Invoke(null, new object?[] { binder, typeof(TService) });
                    else
                    {
                        bindMethodInfo.Invoke(null, new object?[] { binder, null });
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error binding gRPC service '{typeof(TService).Name}'.", ex);
                }
            }
        }
    }
}
