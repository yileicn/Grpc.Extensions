using Grpc.Extension.Abstract.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.Abstract
{
    /// <summary>
    /// Grpc.Extension内部异常
    /// </summary>
    public class InternalException : Exception
    {
        private int Code { get; set; }

        /// <summary>
        /// InternalException
        /// </summary>
        /// <param name="code"></param>
        public InternalException(int code)
        {
            this.SetCode(code);
        }

        /// <summary>
        /// InternalException
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public InternalException(int code, string message) : base(message)
        {
            this.SetCode(code);
        }

        /// <summary>
        /// InternalException
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public InternalException(int code, string message, Exception innerException) : base(message, innerException)
        {
            this.SetCode(code);
        }

        /// <summary>
        /// SetCode
        /// </summary>
        /// <param name="code"></param>
        public void SetCode(int code)
        {
            this.Code = GrpcErrorCode.DefaultErrorCode + code;
            this.Data.Add("ErrorCode", this.Code);
        }
    }
}
