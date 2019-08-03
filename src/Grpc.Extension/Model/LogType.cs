using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.Model
{
    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// 服务端日志
        /// </summary>
        ServerLog = 0,
        /// <summary>
        /// 客户端日志
        /// </summary>
        ClientLog = 1,
    }
}
