using Grpc.Core;
using System;
using System.Threading.Tasks;
using Grpc.Extension.Internal;
using Grpc.Extension.Model;

namespace Grpc.Extension.BaseService
{
    /// <summary>
    /// 执行命令的服务
    /// </summary>
    public class CmdService : IGrpcService
    {
        /// <summary>
        /// 注册服务方法
        /// </summary>
        public void RegisterMethod(ServerServiceDefinition.Builder builder)
        {
            builder.AddMethod(this.BuildMethod<AddDelThrottleRQ, CmdRS>("AddDelThrottle", "grpc", Consts.BaseServiceName), AddDelThrottle);
            builder.AddMethod(this.BuildMethod<AddDelSaveResponseEnableRQ, CmdRS>("AddDelSaveResponseEnable", "grpc", Consts.BaseServiceName), AddDelSaveResponseEnable);
        }
        /// <summary>
        /// 添加删除截流的method
        /// </summary>
        public Task<CmdRS> AddDelThrottle(AddDelThrottleRQ rq, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(rq.MethodName))
                {
                    return CmdRS.Fail("MethodName is null");
                }
                if(rq.IsDel)
                    ThrottleManager.Instance.Del(rq.MethodName);
                else
                    ThrottleManager.Instance.Add(rq.MethodName);
                return CmdRS.Success();
            });
        }
        /// <summary>
        /// 添加删除是否允许保存响应的method
        /// </summary>
        public Task<CmdRS> AddDelSaveResponseEnable(AddDelSaveResponseEnableRQ rq, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(rq.MethodName))
                {
                    return CmdRS.Fail("MethodName is null");
                }
                if (rq.IsDel)
                    MonitorManager.Instance.DelSaveResponseMethod(rq.MethodName);
                else
                    MonitorManager.Instance.AddSaveResponseMethod(rq.MethodName);
                return CmdRS.Success();
            });
        }
    }
}
