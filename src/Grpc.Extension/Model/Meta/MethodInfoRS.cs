using ProtoBuf;

namespace Grpc.Extension.Model
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    internal class MethodInfoRS
    {
        [ProtoMember(1)]
        public string RequestJson { get; set; }

        [ProtoMember(2)]
        public string ResponseJson { get; set; }
    }
}
