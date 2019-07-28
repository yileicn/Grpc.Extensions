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
            //构建RpcException
            var errModel = new ErrorModel
            {
                Code = ParseCode(ex),
                Detail = ex.Message,
                Internal = GetFlatException(ex),
                Status = (int)StatusCode.Internal
            };
            rpcEx = new RpcException(new Status(StatusCode.Internal, errModel.ToJson()));
            rpcEx.Data.Add("ErrorCode", errModel.Code);
            return rpcEx;
        }

        /// <summary>
        /// 返回一个FlatException
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private static string GetFlatException(Exception ex)
        {
            var exception = "";
            if (ex is AggregateException aex)
            {
                foreach (var e in aex.Flatten().InnerExceptions)
                {
                    exception += e?.ToString() + Environment.NewLine;
                }
            }
            else
            {
                exception = ex.ToString();
            }
            return exception;
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
                return GrpcServerOptions.Instance.DefaultErrorCode;
            }
        }
    }
}
