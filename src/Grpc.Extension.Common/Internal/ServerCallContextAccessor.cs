using Grpc.Core;
using System.Threading;

namespace Grpc.Extension.Common.Internal
{
    /// <summary>
    /// ServerCallContextAccessor
    /// </summary>
    public static class ServerCallContextAccessor
    {
        private static readonly AsyncLocal<ServerCallContext> context = new AsyncLocal<ServerCallContext>();

        public static ServerCallContext Current {
            get { return context.Value; }
            set { context.Value = value; }
        }
    }
}
