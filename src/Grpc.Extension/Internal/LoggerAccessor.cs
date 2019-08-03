using Grpc.Extension.Model;
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
        public delegate void LoggerErrorAction(Exception ex, LogType logType = LogType.ServerLog);
        public delegate void LoggerMonitorAction(string msg, LogType logType = LogType.ServerLog);

        private static Lazy<LoggerAccessor> instance = new Lazy<LoggerAccessor>(() => new LoggerAccessor(), true);
        internal static LoggerAccessor Instance
        {
            get { return instance.Value; }
        }
        private LoggerAccessor()
        {
        }

        /// <summary>
        /// 写异常日志
        /// </summary>
        public LoggerErrorAction LoggerError { get; set; }

        /// <summary>
        /// 写监控日志
        /// </summary>
        public LoggerMonitorAction LoggerMonitor { get; set; }
    }
}
