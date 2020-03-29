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
        private readonly TestScope _testScope;
        private readonly GreeterClient _greeterClient;

        public ClientTestGrpc(TestScope testScope, GreeterClient greeterClient)
        {
            _testScope = testScope;
            _greeterClient = greeterClient;
        }
        public async Task<StringMessage> ClientTest(EmptyMessage request, ServerCallContext context)
        {
            var result = await _greeterClient.SayHelloAsync(new Helloworld.HelloRequest() { Name = "yilei" });
            return new StringMessage() { Value = $"{result.Message},guid {_testScope.Id} " };
        }
    }
}
