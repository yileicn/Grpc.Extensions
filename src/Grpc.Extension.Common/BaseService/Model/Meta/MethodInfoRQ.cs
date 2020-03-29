using ProtoBuf;

namespace Grpc.Extension.BaseService.Model
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class MethodInfoRQ
    {
        [ProtoMember(1)]
        public string FullName { get; set; }
    }
}
