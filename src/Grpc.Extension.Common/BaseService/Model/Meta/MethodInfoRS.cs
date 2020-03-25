using ProtoBuf;

namespace Grpc.Extension.BaseService.Model
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class MethodInfoRS
    {
        [ProtoMember(1)]
        public string RequestJson { get; set; }

        [ProtoMember(2)]
        public string ResponseJson { get; set; }
    }
}
