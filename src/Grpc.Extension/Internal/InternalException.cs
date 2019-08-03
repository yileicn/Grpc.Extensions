using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.Internal
{
    /// <summary>
    /// Grpc.Extension内部异常
    /// </summary>
    internal class InternalException : Exception
    {
        public int Code { get; private set; }

        public InternalException(int code)
        {
            this.SetCode(code);
        }
        public InternalException(int code, string message) : base(message)
        {
            this.SetCode(code);
        }
        public InternalException(int code, string message, Exception innerException) : base(message, innerException)
        {
            this.SetCode(code);
        }
        public void SetCode(int code)
        {
            this.Code = GrpcServerOptions.Instance.DefaultErrorCode + code;
            this.Data.Add("ErrorCode", this.Code);
        }
    }
}
