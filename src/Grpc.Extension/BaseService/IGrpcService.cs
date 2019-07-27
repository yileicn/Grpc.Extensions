using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Core;

namespace Grpc.Extension.BaseService
{
    /// <summary>
    /// GrpcService
    /// </summary>
    public interface IGrpcService
    {
        /// <summary>
        /// 注册服务方法
        /// </summary>
        void RegisterMethod(ServerServiceDefinition.Builder builder);
    }

    /// <summary>
    /// 基础服务
    /// </summary>
    public interface IGrpcBaseService : IGrpcService
    {

    }
}
