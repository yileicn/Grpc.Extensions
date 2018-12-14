using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace Math.Model
{
    [ProtoContract]
    public class AddRequest
    {
        [ProtoMember(1)]
        public int Num1 { get; set; }

        [ProtoMember(2)]
        public int Num2 { get; set; }
    }
}
