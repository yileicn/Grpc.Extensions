using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Extension.Abstract;
using Grpc.Extension.Abstract.Model;
using Grpc.Extension.Common;
using Grpc.Extension.Common.Internal;
using System;
using System.Linq;

namespace Grpc.Extension.Client.Interceptors
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

            var result = continuation(request, context);
            var respAsync = result.ResponseAsync.ContinueWith(action => {
                try
                {
                    var response = action.Result;
                    model.Status = "ok";
                    //model.ResponseData = response.ToJson();
                    return response;
                }
                catch (AggregateException aex)
                {
                    var ex = aex.InnerException;
                    SetExceptionData(ex, model);
                    model.Status = "error";
                    model.Exception = aex.GetFlatException();
                    LoggerAccessor.Instance.OnLoggerError(ex, LogType.ClientLog);
                    throw ex;
                }
                finally
                {
                    model.ResponseTime = DateTime.Now;
                    LoggerAccessor.Instance.OnLoggerMonitor(model.ToJson(), LogType.ClientLog);
                }
            });
            //var response = result.GetAwaiter().GetResult();
                
            return new AsyncUnaryCall<TResponse>(respAsync, result.ResponseHeadersAsync, result.GetStatus, result.GetTrailers, result.Dispose);
            
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
                return result;
            }
            catch (Exception ex)
            {
                SetExceptionData(ex, model);
                model.Status = "error";                
                model.Exception = ex.GetFlatException();
                LoggerAccessor.Instance.OnLoggerError(ex, LogType.ClientLog);
                throw ex;
            }
            finally
            {
                model.ResponseTime = DateTime.Now;
                LoggerAccessor.Instance.OnLoggerMonitor(model.ToJson(), LogType.ClientLog);
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

        private void SetExceptionData(Exception ex, MonitorModel model)
        {
            var dataRequest = ex.Data["Request"];
            if (dataRequest != null)
            {
                model.Items.TryAdd("OriginRequest", dataRequest);
                ex.Data["Request"] = model;
            }
            else
            {
                ex.Data.Add("Request", model);
            }
        }
    }
}
