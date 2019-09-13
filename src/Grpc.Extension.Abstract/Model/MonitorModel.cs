using System;
using System.Collections.Generic;
using System.Collections.Concurrent;


namespace Grpc.Extension.Abstract.Model
{
    /// <summary>
    /// 日志监控实体
    /// </summary>
    [Serializable]
    public class MonitorModel
    {
        /// <summary>
        /// MonitorModel
        /// </summary>
        public MonitorModel()
        {
            RequestId = Guid.NewGuid().ToString();
            RequestTime = DateTime.Now;
        }

        /// <summary>
        /// 请求Id
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// 客户端Ip
        /// </summary>
        public string ClientIp { get; set; }
        
        /// <summary>
        /// 请求时间
        /// </summary>
        public DateTime RequestTime { get; set; }

        /// <summary>
        /// 请求Url
        /// </summary>
        public string RequestUrl { get; set; }

        /// <summary>
        /// 请求参数
        /// </summary>
        public string RequestData { get; set; }

        /// <summary>
        /// 请求头
        /// </summary>
        public Dictionary<string, string> RequestHeaders { get; set; }
        /// <summary>
        /// 多层调用的追踪id
        /// </summary>
        public string TraceId { get; set; }


        /// <summary>
        /// ok | error
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 响应时间
        /// </summary>
        public DateTime ResponseTime { get; set; }

        /// <summary>
        /// 响应数据
        /// </summary>
        public string ResponseData { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public string Exception { get; set; }

        /// <summary>
        /// 总耗时
        /// </summary>
        public double TotalElapsed => (ResponseTime - RequestTime).TotalMilliseconds;

        /// <summary>
        /// 访问上下信息的预留属性
        /// </summary>
        public ConcurrentDictionary<string, object> Items { get; set; } = new ConcurrentDictionary<string, object>();
    }
}
