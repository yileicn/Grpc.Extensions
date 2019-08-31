using ProtoBuf;

namespace Grpc.Extension.Model
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    internal class MethodInvokeRS
    {
        public string ResponseJson { get; set; }
    }
}
