using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Extension.BaseService.Model;
using System.Linq;
using Grpc.Extension.Common;

namespace Grpc.Extension.BaseService
{
    /// <summary>
    /// Grpc元数据服务
    /// </summary>
    public class MetaService : IGrpcBaseService
    {
        /// <summary>
        /// 服务基本信息
        /// </summary>
        public Task<InfoRS> Info(InfoRQ rq, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var methods = MetaModel.Methods.Select(q => q.FullName?.Trim()).ToList();
                if (!string.IsNullOrWhiteSpace(rq.MethodName))
                {
                    methods = methods?.Where(q => q.ToLower().Contains(rq.MethodName.Trim().ToLower())).ToList();
                }
                var methodInfos = new List<GrpcMethodInfo>();
                foreach (var m in methods)
                {
                    var info = new GrpcMethodInfo { Name = m };
                    info.IsThrottled = ThrottleManager.Instance.IsThrottled(m);
                    info.SaveResponseEnable = MonitorManager.Instance.SaveResponseMethodEnable(m);
                    methodInfos.Add(info);
                }
                return new InfoRS
                {
                    IpAndPort = $"{MetaModel.Ip}:{MetaModel.Port}",
                    StartTime = MetaModel.StartTime.ToUnixTimestamp(),
                    MethodInfos = methodInfos
                };
            });
        }
        /// <summary>
        /// 服务方法的详细信息
        /// </summary>
        public Task<MethodInfoRS> MethodInfo(MethodInfoRQ rq, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var methodInfo = MetaModel.Methods.FirstOrDefault(q => q.FullName == rq.FullName?.Trim());
                if (methodInfo == null)
                {
                    return new MethodInfoRS();
                }
                return new MethodInfoRS
                {
                    RequestJson = Activator.CreateInstance(methodInfo.RequestType).FillProp().ToJson(ignoreNullValue: false, isIndented: true),
                    ResponseJson = Activator.CreateInstance(methodInfo.ResponseType).FillProp().ToJson(ignoreNullValue: false, isIndented: true)
                };
            });
        }
        /// <summary>
        /// 服务方法调用
        /// </summary>
        public Task<MethodInvokeRS> MethodInvoke(MethodInvokeRQ rq, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                var methodInfo = MetaModel.Methods.FirstOrDefault(q => q.FullName == rq.FullName?.Trim());
                if (methodInfo == null)
                {
                    return new MethodInvokeRS() { ResponseJson = $"not fount method by fullname:{rq.FullName}" };
                }
                var task = (Task)methodInfo.Handler.DynamicInvoke(rq.RequestJson?.Trim().FromJson(methodInfo.RequestType), context);
                task.Wait();
                dynamic result = task;
                return new MethodInvokeRS { ResponseJson = ((object)result.Result).ToJson(ignoreNullValue: false, isIndented: true) };
            });
        }
    }
}
