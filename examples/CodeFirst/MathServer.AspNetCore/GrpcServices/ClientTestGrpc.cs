using Grpc.Core;
using Grpc.Extension.Abstract;
using Grpc.Extension.Common.Internal;
using Math;
using System.Threading.Tasks;
using static Helloworld.Greeter;

namespace MathServer.AspNetCore
{
    public class ClientTestGrpc : IGrpcService
    {
        public async Task<StringMessage> ClientTest(EmptyMessage request, ServerCallContext context)
        {
            var greeterClient = ServiceProviderAccessor.GetService<GreeterClient>();
            var result = await greeterClient.SayHelloAsync(new Helloworld.HelloRequest() { Name = "yilei" });
            return new StringMessage() { Value = result.Message };
        }
    }
}
