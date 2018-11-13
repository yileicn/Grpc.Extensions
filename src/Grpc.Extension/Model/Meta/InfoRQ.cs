
using ProtoBuf;

namespace Grpc.Extension.Model
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class InfoRQ
    {
        [ProtoMember(1)]
        public string MethodName { get; set; }
    }
}
