using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Math.Model
{
    [ProtoContract]
    public class SumRequest
    {        
        [ProtoMember(1)]
        public int Num { get; set; }
    }
}
