using Grpc.Core;
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
    }
}
