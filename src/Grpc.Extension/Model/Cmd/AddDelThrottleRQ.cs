using ProtoBuf;

namespace Grpc.Extension.Model
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class AddDelThrottleRQ
    {
        [ProtoMember(1)]
        public string MethodName { get; set; }

        [ProtoMember(2)]
        public bool IsDel { get; set; }
    }
}
