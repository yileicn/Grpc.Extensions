using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Grpc.Extension.Internal
{
    public static class ServerCallContextAccessor
    {
        private static readonly AsyncLocal<ServerCallContext> context = new AsyncLocal<ServerCallContext>();

        public static ServerCallContext Current {
            get { return context.Value; }
            set { context.Value = value; }
        }
    }
}
