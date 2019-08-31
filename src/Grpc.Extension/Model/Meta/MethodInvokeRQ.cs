using ProtoBuf;

namespace Grpc.Extension.Model
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    internal class MethodInvokeRQ
    {
        [ProtoMember(1)]
        public string FullName { get; set; }

        [ProtoMember(2)]
        public string RequestJson { get; set; }
    }
}
