using ProtoBuf;

namespace Grpc.Extension.Model
{
    /// <summary>
    /// MethodInvokeRQ
    /// </summary>
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    internal class MethodInvokeRQ
    {
        /// <summary>
        /// GrpcMethod FullName
        /// </summary>
        [ProtoMember(1)]
        public string FullName { get; set; }

        /// <summary>
        /// RequestJson
        /// </summary>
        [ProtoMember(2)]
        public string RequestJson { get; set; }
    }
}
