using ProtoBuf;
using System.Collections.Generic;

namespace Grpc.Extension.Model
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    internal class InfoRS
    {
        [ProtoMember(1)]
        public string IpAndPort { get; set; }

        [ProtoMember(2)]
        public long StartTime { get; set; }

        [ProtoMember(3)]
        public List<MethodInfo> MethodInfos { get; set; }
    }

    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    internal class MethodInfo
    {
        public string Name { get; set; }

        public bool SaveResponseEnable { get; set; }

        public bool IsThrottled { get; set; }
    }
}
