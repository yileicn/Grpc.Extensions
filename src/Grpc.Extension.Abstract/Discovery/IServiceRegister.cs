using Grpc.Extension.Abstract.Model;

namespace Grpc.Extension.Abstract.Discovery
{
    /// <summary>
    /// 服务注册
    /// </summary>
    public interface IServiceRegister
    {
        /// <summary>
        /// 服务注册
        /// </summary>
        /// <param name="model"></param>
        void RegisterService(ServiceRegisterModel model);

        /// <summary>
        /// 服务反注册
        /// </summary>
        void DeregisterService();
    }
}