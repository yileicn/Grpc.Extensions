using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GreeterServer.Common;
using Grpc.Core;
using Grpc.Core.Utils;
using Helloworld;

namespace GreeterServer
{
    public class GreeterImpl : Greeter.GreeterBase
    {
        // Server side handler of the SayHello RPC
        public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            var client = ScopeServiceProvider.GetService<MathGrpc.MathGrpc.MathGrpcClient>();
            //调用服务端流方法
            //var result = client.SumServerStream(new MathGrpc.SumRequest() { Num = 10 });
            //await result.ResponseStream.ForEachAsync(res =>
            //{
            //    Console.Write(res.Value);
            //    return Task.CompletedTask;
            //});

            //调用客务端流方法
            //var result = client.Sum();
            //await result.RequestStream.WriteAsync(new MathGrpc.SumRequest() { Num = 14 });
            //await result.RequestStream.WriteAsync(new MathGrpc.SumRequest() { Num = 15 });
            //await result.RequestStream.CompleteAsync();
            //Console.WriteLine(result.ResponseAsync.Result);
            return new HelloReply { Message = "Hello " + request.Name };
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
