using ProtoBuf;

namespace Grpc.Extension.Model
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CmdRS
    {
        [ProtoMember(1)]
        public bool Result { get; set; }

        [ProtoMember(2)]
        public string Message { get; set; }

        public static CmdRS Success(string msg = null)
        {
            return new CmdRS { Result = true, Message = msg };
        }

        public static CmdRS Fail(string msg)
        {
            return new CmdRS { Result = false, Message = msg };
        }
    }
}
