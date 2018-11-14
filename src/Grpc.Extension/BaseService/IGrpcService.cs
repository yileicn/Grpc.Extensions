using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;

namespace Grpc.Extension.BaseService
{
    public interface IGrpcService
    {
        /// <summary>
        /// 注册服务方法
        /// </summary>
        void RegisterMethod(ServerServiceDefinition.Builder builder);
    }
}
