using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace Math.Model
{
    /// <summary>
    /// 加法请求参数
    /// </summary>
    [ProtoContract]
    public class AddRequest
    {
        /// <summary>
        /// 第一个数字
        /// </summary>
        [ProtoMember(1)]
        public int Num1 { get; set; }

        /// <summary>
        /// 第二个数字
        /// </summary>
        [ProtoMember(2)]
        public int Num2 { get; set; }
    }
}
