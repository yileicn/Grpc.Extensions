using Grpc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FM.GrpcDashboard.Services
{
    public class GrpcServiceProxy : IGrpcReflection
    {
        private readonly IEnumerable<IGrpcReflection> grpcServices;
        private ConcurrentDictionary<string, string> dicGrpcType = new ConcurrentDictionary<string, string>();

        public GrpcServiceProxy(IEnumerable<IGrpcReflection> grpcServices)
        {
            this.grpcServices = grpcServices;
        }

        public async Task<InfoRS> GetInfo(string address, int port)
        {
            var endpoint = $"{address}:{port}";
            foreach (var grpcService in getGrpcSevices(endpoint))
            {
                try
                {
                    var result = await grpcService.GetInfo(address, port);
                    var typeName = grpcService.GetType().Name;
                    dicGrpcType.AddOrUpdate(endpoint, typeName, (k,v) => typeName);
                    return result;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return null;
        }

        public async Task<MethodInfoRS> GetMethodInfo(string endpoint, string methodName)
        {
            foreach (var grpcService in getGrpcSevices(endpoint))
            {
                try
                {
                    return await grpcService.GetMethodInfo(endpoint, methodName);
                }
                catch (Exception)
                {
                    continue;
                }
            }
            return null;
        }

        public async Task<string> MethodInvoke(string endpoint, string methodName, string requestJson, Dictionary<string, string> customHeaders)
        {
            Exception lastException = new Exception();
            foreach (var grpcService in getGrpcSevices(endpoint))
            {
                try
                {
                    return await grpcService.MethodInvoke(endpoint, methodName, requestJson, customHeaders);
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    continue;
                }
            }
            return lastException.ToString();
        }

        private IEnumerable<IGrpcReflection> getGrpcSevices(string endpoint)
        {
            
            var grpcType = dicGrpcType.GetValueOrDefault(endpoint);
            if (string.IsNullOrEmpty(grpcType))
            {
                return grpcServices;
            }
            else
            {
                return grpcServices.Where(p => p.GetType().Name == grpcType);
            }
        }

        public Tuple<bool, string> SaveResponse(string serviceName, string methodName, bool isSaveResponse)
        {
            throw new NotImplementedException();
        }

        public Tuple<bool, string> Throttle(string serviceName, string methodName, bool isThrottle)
        {
            throw new NotImplementedException();
        }
    }
}
