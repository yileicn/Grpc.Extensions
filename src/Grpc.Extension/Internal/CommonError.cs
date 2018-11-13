using Grpc.Core;
using Grpc.Extension.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Grpc.Extension.Common;

namespace Grpc.Extension.Internal
{
    /// <summary>
    /// 统一错误构建
    /// </summary>
    public class CommonError
    {
        /// <summary>
        /// 返回一个rpc异常到客户端
        /// </summary>
        public static RpcException BuildRpcException(Exception ex)
        {
            if (ex is RpcException rpcEx)
            {
                return rpcEx;
            }

            var errModel = new ErrorModel
            {
                Code = ParseCode(ex),
                Detail = ex.Message,
                Internal = ex.ToString(),
                Status = (int)StatusCode.Internal
            };
            return new RpcException(new Status(StatusCode.Internal, errModel.ToJson()));
        }

        private static int ParseCode(Exception ex)
        {
            try
            {
                dynamic d = ex;
                var code = d.Code;
                return code;
            }
            catch
            {
                return 1;
            }
        }
    }
}
