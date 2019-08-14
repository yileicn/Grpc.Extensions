using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Grpc.Core.Interceptors;
using Grpc.Extension.Interceptors;
using GreeterServer.Common;

namespace GreeterServer.Middlewares
{
    public class RequestServicesMiddleware : ServerInterceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
            ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            using (var scope = Common.AppServiceProvider.ServiceProvider.CreateScope())
            {
                ScopeServiceProvider.Current = scope.ServiceProvider;
                var res = await continuation(request, context);
                ScopeServiceProvider.Current = null;
                return res;
            }
        }
    }
}
