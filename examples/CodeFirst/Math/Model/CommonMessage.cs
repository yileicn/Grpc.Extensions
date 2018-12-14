using ProtoBuf;
using System.Collections.Generic;

namespace Math
{
    [ProtoContract]
    public class EmptyMessage
    {
    }

    [ProtoContract]
    public class BoolMessage
    {
        [ProtoMember(1)]
        public bool Value { get; set; }
    }

    [ProtoContract]
    public class IntMessage
    {
        [ProtoMember(1)]
        public int Value { get; set; }
    }

    [ProtoContract]
    public class DoubleMessage
    {
        [ProtoMember(1)]
        public double Value { get; set; }
    }

    [ProtoContract]
    public class StringMessage
    {
        [ProtoMember(1)]
        public string Value { get; set; }
    }
}