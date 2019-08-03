using System;
using System.Collections.Generic;
using System.Collections.Concurrent;


namespace Grpc.Extension.Model
{
    /// <summary>
    /// 日志监控实体
    /// </summary>
    [Serializable]
    public class MonitorModel
    {
        public MonitorModel()
        {
            RequestId = Guid.NewGuid().ToString();
            RequestTime = DateTime.Now;
        }

        public string RequestId { get; set; }

        public string ClientIp { get; set; }
        
        public DateTime RequestTime { get; set; }

        public string RequestUrl { get; set; }

        public string RequestData { get; set; }
        /// <summary>
        /// 多层调用的追踪id
        /// </summary>
        public string TraceId { get; set; }


        /// <summary>
        /// ok | error
        /// </summary>
        public string Status { get; set; }

        public DateTime ResponseTime { get; set; }

        public string ResponseData { get; set; }

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
