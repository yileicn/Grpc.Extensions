using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Utils;
using Helloworld;
using Microsoft.Extensions.Logging;

namespace MathServer.AspNetCore
{
    public class GreeterGrpcImpl : Greeter.GreeterBase
    {
        private readonly TestScope _testScope;

        public GreeterGrpcImpl(TestScope testScope)
        {
            _testScope = testScope;
        }
        // Server side handler of the SayHello RPC
        public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return new HelloReply { Message = $"Hello {request.Name}, guid {_testScope.Id}" };
        }

        public override async Task<HelloReply> SayHelloStream(IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
        {
            var helloReply = new HelloReply();
            var sb = new StringBuilder();
            await requestStream.ForEachAsync(req =>
            {
                sb.AppendLine("Hello " + requestStream.Current.Name);
                return Task.CompletedTask;
            });
            //while (await requestStream.MoveNext())
            //{
            //    sb.AppendLine("Hello " + requestStream.Current.Name);
            //}
            helloReply.Message = sb.ToString();
            return helloReply;
        }
    }
}
