using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.Abstract
{
    /// <summary>
    /// GrpcService(CodeFirst)
    /// </summary>
    public interface IGrpcService
    {

    }

    /// <summary>
    /// 非Grpc方法
    /// </summary>
    public sealed class NotGrpcMethodAttribute : Attribute { }
}
