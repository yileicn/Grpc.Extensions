
using ProtoBuf;

namespace Grpc.Extension.Model
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    internal class InfoRQ
    {
        [ProtoMember(1)]
        public string MethodName { get; set; }
    }
}
