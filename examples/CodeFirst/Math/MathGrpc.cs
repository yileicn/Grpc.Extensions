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
        /// <summary>
        /// 加法
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<IntMessage> Add(AddRequest request, ServerCallContext context)
        {
            var result = new IntMessage();
            result.Value = request.Num1 + request.Num2;
            return Task.FromResult(result);
        }

        /// <summary>
        /// 减法
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<IntMessage> Sub(SubRequest request, ServerCallContext context)
        {
            var result = new IntMessage();
            result.Value = request.Num1 - request.Num2;
            return Task.FromResult(result);
        }

        /// <summary>
        /// 客户端流求和
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 服务端流求和
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task SumServerStream(SumRequest request, IServerStreamWriter<IntMessage> responseStream, ServerCallContext context)
        {
            var result = new IntMessage();
            result.Value = request.Num;
            await responseStream.WriteAsync(result);
        }
    }
}
