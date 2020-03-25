using ProtoBuf;
using System.Collections.Generic;

namespace Grpc.Extension.BaseService.Model
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class InfoRS
    {
        [ProtoMember(1)]
        public string IpAndPort { get; set; }

        [ProtoMember(2)]
        public long StartTime { get; set; }

        [ProtoMember(3)]
        public List<GrpcMethodInfo> MethodInfos { get; set; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class GrpcMethodInfo
    {
        public string Name { get; set; }

        public bool SaveResponseEnable { get; set; }

        public bool IsThrottled { get; set; }
    }
}
