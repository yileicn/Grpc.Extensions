using Grpc.Core;
using System;
using Grpc.Extension.Abstract.Model;

namespace Grpc.Extension.Common
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
            if (ex is RpcException)
            {
                return ex as RpcException;
            }
            else if (ex.InnerException is RpcException)
            {
                return ex.InnerException as RpcException;
            }
            //构建RpcException
            var errModel = new ErrorModel
            {
                Code = ParseCode(ex),
                Detail = ex.Message,
                Internal = ex.GetFlatException(),
                Status = (int)StatusCode.Internal
            };
            var rpcEx = new RpcException(new Status(StatusCode.Internal, errModel.ToJson()));
            rpcEx.Data.Add("ErrorCode", errModel.Code);
            return rpcEx;
        }

        private static int ParseCode(Exception ex)
        {
            try
            {
                dynamic d = ex.InnerException == null ? ex : ex.InnerException;
                var code = d.Code;
                return code;
            }
            catch
            {
                return GrpcErrorCode.DefaultErrorCode + GrpcErrorCode.Internal;
            }
        }
    }
}
