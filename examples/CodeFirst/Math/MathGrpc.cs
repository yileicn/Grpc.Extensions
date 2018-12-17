using Grpc.Core;
using Grpc.Core.Utils;
using Grpc.Extension.BaseService;
using Math.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Math
{
    public class MathGrpc : IGrpcService
    {
        public void RegisterMethod(ServerServiceDefinition.Builder builder)
        {
            builder.AddMethod(this.BuildMethod<AddRequest, IntMessage>("Add", null), Add);
            builder.AddMethod(this.BuildMethod<SubRequest, IntMessage>("Sub", null), Sub);
            builder.AddMethod(this.BuildMethod<SumRequest, IntMessage>("Sum", null,mType: MethodType.ClientStreaming), Sum);
        }

        public Task<IntMessage> Add(AddRequest request, ServerCallContext context)
        {
            var result = new IntMessage();
            result.Value = request.Num1 + request.Num2;
            return Task.FromResult(result);
        }

        public Task<IntMessage> Sub(SubRequest request, ServerCallContext context)
        {
            var result = new IntMessage();
            result.Value = request.Num1 - request.Num2;
            return Task.FromResult(result);
        }

        public async Task<IntMessage> Sum(IAsyncStreamReader<SumRequest> request, ServerCallContext context)
        {
            var result = new IntMessage();
            int sum = 0;
            await request.ForEachAsync(req =>
            {
                sum += req.Num;
                return Task.CompletedTask;
            });
            result.Value = sum;
            return result;
        }
    }
}
