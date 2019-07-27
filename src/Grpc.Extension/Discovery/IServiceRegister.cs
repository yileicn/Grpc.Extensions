namespace Grpc.Extension.Discovery
{
    /// <summary>
    /// 服务注册
    /// </summary>
    public interface IServiceRegister
    {
        /// <summary>
        /// 注册
        /// </summary>
        void RegisterService();

        /// <summary>
        /// 反注册
        /// </summary>
        void DeregisterService();
    }
}