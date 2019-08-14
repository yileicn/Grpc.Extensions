using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Extension.Common;
using Grpc.Extension.Internal;
using Grpc.Extension.Model;
using System;
using System.Linq;

namespace Grpc.Extension.Interceptors
{
    internal class ClientMonitorInterceptor : ClientInterceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            context = SetTraceIdHeader(context);
            var model = new MonitorModel
            {
                ClientIp = context.Host,
                RequestUrl = context.Method.FullName,
                RequestData = request?.ToJson(),
                RequestHeaders = context.Options.Headers.ToDictionary(p => p.Key, p => p.Value),
                TraceId = context.Options.Headers?.Where(p => p.Key == Consts.TraceId).FirstOrDefault()?.Value
            };
            try
            {
                var result = continuation(request, context);
                var data = result.GetAwaiter().GetResult();
                //model.ResponseData = data.ToJson();
                model.Status = "ok";
                model.ResponseTime = DateTime.Now;
                return result;
            }
            catch (Exception ex)
            {
                ex.Data.Add("Request", model);
                model.Status = "error";
                model.ResponseTime = DateTime.Now;
                model.Exception = CommonError.GetFlatException(ex);
                LoggerAccessor.Instance.LoggerError?.Invoke(ex, LogType.ClientLog);
                throw ex;
            }
            finally
            {
                LoggerAccessor.Instance.LoggerMonitor?.Invoke(model.ToJson(), LogType.ClientLog);
            }
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            context = SetTraceIdHeader(context);
            var model = new MonitorModel
            {
                ClientIp = context.Host,
                RequestUrl = context.Method.FullName,
                RequestData = request?.ToJson(),
                RequestHeaders = context.Options.Headers.ToDictionary(p => p.Key, p => p.Value),
                TraceId = context.Options.Headers?.Where(p => p.Key == Consts.TraceId).FirstOrDefault()?.Value
            };
            try
            {
                var result = continuation(request, context);
                model.Status = "ok";
                model.ResponseTime = DateTime.Now;
                return result;
            }
            catch (Exception ex)
            {
                ex.Data.Add("Request", model);
                model.Status = "error";
                model.ResponseTime = DateTime.Now;
                model.Exception = CommonError.GetFlatException(ex);
                LoggerAccessor.Instance.LoggerError?.Invoke(ex, LogType.ClientLog);
                throw ex;
            }
            finally
            {
                LoggerAccessor.Instance.LoggerMonitor?.Invoke(model.ToJson(), LogType.ClientLog);
            }
        }

        private ClientInterceptorContext<TRequest, TResponse> SetTraceIdHeader<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest : class
            where TResponse : class
        {
            var header = context.Options.Headers?.Where(p => p.Key == Consts.TraceId).FirstOrDefault();
            if (header == null)
            {
                var serverHeader = ServerCallContextAccessor.Current?.RequestHeaders.Where(p => p.Key == Consts.TraceId).FirstOrDefault();
                var traceId = serverHeader == null ? Guid.NewGuid().ToString() : serverHeader.Value;
                header = new Metadata.Entry(Consts.TraceId, traceId);
                if (context.Options.Headers == null)
                {
                    var meta = new Metadata();
                    meta.Add(header);
                    var callOptions = context.Options.WithHeaders(meta);
                    context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host, callOptions);
                }
                else
                {
                    context.Options.Headers.Add(header);
                }
            }
            return context;
        }
    }
}
