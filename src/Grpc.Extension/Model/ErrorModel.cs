namespace Grpc.Extension.Model
{
    /// <summary>
    /// 统一错误模型
    /// </summary>
    public class ErrorModel
    {
        public int Code { get; set; }
        public int Status { get; set; }
        public string Detail { get; set; }
        public string Internal { get; set; }
        public string Content { get; set; }
    }
}
