using ProtoBuf;

namespace Grpc.Extension.BaseService.Model
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class MethodInvokeRS
    {
        public string ResponseJson { get; set; }
    }
}
