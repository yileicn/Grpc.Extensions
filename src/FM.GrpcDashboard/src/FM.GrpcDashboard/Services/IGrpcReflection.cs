using Grpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FM.GrpcDashboard.Services
{
    public interface IGrpcReflection
    {
        Task<InfoRS> GetInfo(string address, int port);

        Task<MethodInfoRS> GetMethodInfo(string endpoint, string methodName);

        Task<string> MethodInvoke(string endpoint, string methodName, string requestJson, Dictionary<string, string> customHeaders);

        Tuple<bool, string> Throttle(string serviceName, string methodName, bool isThrottle);

        Tuple<bool, string> SaveResponse(string serviceName, string methodName, bool isSaveResponse);

    }
}
