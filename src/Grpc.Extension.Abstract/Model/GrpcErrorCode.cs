using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.Abstract.Model
{
    /// <summary>
    /// GrpcErrorCode
    /// </summary>
    public class GrpcErrorCode
    {
        /// <summary>
        /// 默认错误码
        /// </summary>
        public static int DefaultErrorCode = 1;

        /// <summary>
        /// 内部异常
        /// </summary>
        public const int Internal = 0;

    }
}
