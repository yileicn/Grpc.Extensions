using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.BaseService.Model
{
    internal class XmlCommentInfo
    {
        /// <summary>
        /// FullName(Math.MathGrpc.Add(Math.Model.AddRequest,Grpc.Core.ServerCallContext))
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Type(T:类，M:方法，P:属性，F:字段，E:事件）
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Summary(注释)
        /// </summary>
        public string Summary { get; set; }
    }
}
