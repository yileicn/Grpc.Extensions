using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grpc.Extension.Abstract.Discovery
{
    /// <summary>
    /// 服务地址和端口发生改变
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="healthEndpoints"></param>
    public delegate void ServiceChangedEvent(string serviceName, List<string> healthEndpoints);

    /// <summary>
    /// 服务发现
    /// </summary>
    public interface IServiceDiscovery
    {
        /// <summary>
        /// 服务地址和端口发生改变
        /// </summary>
        event ServiceChangedEvent ServiceChanged;

        /// <summary>
        /// 获取健康的服务地址列表
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="discoveryUrl"></param>
        /// <param name="serviceTag"></param>
        /// <returns></returns>
        Task<List<string>> GetEndpoints(string serviceName, string discoveryUrl, string serviceTag);
    }
}