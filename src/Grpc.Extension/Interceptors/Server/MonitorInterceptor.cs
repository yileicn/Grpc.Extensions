using Grpc.Core.Interceptors;
using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;
using Grpc.Extension.Model;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Extension.Common;
using Grpc.Extension.Internal;

namespace Grpc.Extension.Interceptors
{
    /// <summary>
    /// 性能监控,记录日志
    /// </summary>
    public class MonitorInterceptor : ServerInterceptor
    {
        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
            ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
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
                RequestData = request?.ToJson(),
                TraceId = trace.Value
            };
            try
            {
                var result = continuation(request, context);
                model.Status = "ok";
                
                model.ResponseData = MonitorManager.Instance.SaveResponseMethodEnable(context.Method) ? result?.ToJson() : Consts.NotResponseMsg;

                return result;
            }
            catch (Exception ex)
            {
                if (ex is AggregateException aex)
                {
                    foreach (var e in aex.Flatten().InnerExceptions)
                    {
                        model.Exception += e?.ToString() + Environment.NewLine;
                    }
                }
                else
                {
                    model.Exception = ex?.ToString();
                }

                model.Status = "error";
                LoggerAccessor.Instance.LoggerError?.Invoke(new Exception(model.Exception));
                throw CommonError.BuildRpcException(ex);
            }
            finally
            {
                model.ResponseTime = DateTime.Now;
                LoggerAccessor.Instance.LoggerMonitor?.Invoke(model.ToJson());
            }
        }

    }
}
