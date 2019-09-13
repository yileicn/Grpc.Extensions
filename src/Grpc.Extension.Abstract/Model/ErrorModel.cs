namespace Grpc.Extension.Abstract.Model
{
    /// <summary>
    /// 统一错误模型
    /// </summary>
    public class ErrorModel
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// Exception.Message
        /// </summary>
        public string Detail { get; set; }
        /// <summary>
        /// FlatException
        /// </summary>
        public string Internal { get; set; }
        /// <summary>
        /// 备用
        /// </summary>
        public string Content { get; set; }
    }
}
