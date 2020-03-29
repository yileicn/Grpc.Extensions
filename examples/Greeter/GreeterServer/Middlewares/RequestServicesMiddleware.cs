using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Grpc.Extension.Interceptors;
using GreeterServer.Common;
using Grpc.Extension.Common.Internal;

namespace GreeterServer.Middlewares
{
    /// <summary>
    /// 每次请求创建一个Scope
    /// </summary>
    public class RequestServicesMiddleware : ServerInterceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
            ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            using (var scope = ServiceProviderAccessor.ServiceProvider.CreateScope())
            {
                ScopeServiceProvider.Current = scope.ServiceProvider;
                var res = await continuation(request, context);
                ScopeServiceProvider.Current = null;
                return res;
            }
        }
    }
}
