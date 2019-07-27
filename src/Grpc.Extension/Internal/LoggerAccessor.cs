using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.Internal
{
    /// <summary>
    /// 日志访问
    /// </summary>
    public class LoggerAccessor
    {
        private static Lazy<LoggerAccessor> instance = new Lazy<LoggerAccessor>(() => new LoggerAccessor(), true);
        internal static LoggerAccessor Instance
        {
            get { return instance.Value; }
        }
        private LoggerAccessor()
        {
        }

        /// <summary>
        /// 写调试日志
        /// </summary>
        public Action<string> LoggerTrace { get; set; }

        /// <summary>
        /// 写异常日志
        /// </summary>
        public Action<Exception> LoggerError { get; set; }

        /// <summary>
        /// 写监控日志
        /// </summary>
        public Action<string> LoggerMonitor { get; set; }
    }
}
