using System;
using System.Collections.Generic;
using Grpc.Core;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Extension.Common;
using Grpc.Extension.Internal;
using Grpc.Extension.Abstract;
using Grpc.Extension.Abstract.Model;
using Grpc.Extension.Common.Internal;

namespace Grpc.Extension.Interceptors
{
    /// <summary>
    /// 性能监控,记录日志
    /// </summary>
    internal class MonitorInterceptor : ServerInterceptor
    {
        private async Task<TResponse> Monitor<TRequest, TResponse>(object request, 
            ServerCallContext context, Delegate continuation, object response = null)
        {
            ServerCallContextAccessor.Current = context;
            var trace = context.RequestHeaders.FirstOrDefault(q => q.Key == Consts.TraceId);
            if (trace == null)
            {
                trace = new Metadata.Entry(Consts.TraceId, Guid.NewGuid().ToString());
                context.RequestHeaders.Add(trace);
            }
            var model = new MonitorModel
            {
                ClientIp = context.Peer,
                RequestUrl = context.Method,
                //RequestData = request?.ToJson(),
                RequestHeaders = context.RequestHeaders.ToDictionary(p => p.Key, p => p.Value),
                TraceId = trace.Value
            };
            if (request is TRequest)
            {
                model.RequestData = request?.ToJson();
            }
            else if(request is IAsyncStreamReader<TRequest>)
            {
                var requests = new List<TRequest>();
                //await requestStream.ForEachAsync(req=> {
                //    requests.Add(req);
                //    return Task.CompletedTask;
                //});
                model.RequestData = requests?.ToJson();
            }
            try
            {
                if (response == null)
                {
                    var result = await (continuation.DynamicInvoke(request, context) as Task<TResponse>);
                    model.Status = "ok";

                    model.ResponseData = MonitorManager.Instance.SaveResponseMethodEnable(context.Method) ? result?.ToJson() : ServerConsts.NotResponseMsg;

                    return result;
                }
                else
                {
                    await (continuation.DynamicInvoke(request, response, context) as Task);
                    return default(TResponse);
                }
            }
            catch (Exception ex)
            {
                var rpcEx = CommonError.BuildRpcException(ex);
                var dataRequest = rpcEx.Data["Request"];
                if (dataRequest != null)
                {
                    model.Items.TryAdd("ClientRequest", dataRequest);
                    rpcEx.Data["Request"] = model;
                }
                else
                {
                    rpcEx.Data.Add("Request", model);
                }
                model.Exception = rpcEx.ToString();
                model.Status = "error";
                LoggerAccessor.Instance.LoggerError?.Invoke(rpcEx);
                throw rpcEx;
            }
            finally
            {
                ServerCallContextAccessor.Current = null;
                model.ResponseTime = DateTime.Now;
                LoggerAccessor.Instance.LoggerMonitor?.Invoke(model.ToJson());
            }
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
            ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            return await Monitor<TRequest, TResponse>(request, context, continuation);
        }

        public override async Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream,
            ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            return await Monitor<TRequest, TResponse>(requestStream, context, continuation);
        }

        public override async Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            await Monitor<TRequest, TResponse>(request, context, continuation,responseStream);
        }

        public override async Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            await Monitor<TRequest, TResponse>(requestStream, context, continuation, responseStream);
        }
    }
}
