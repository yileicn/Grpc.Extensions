using Grpc.Extension.Abstract.Model;
using System;

namespace Grpc.Extension.Abstract
{
    /// <summary>
    /// 日志访问
    /// </summary>
    public class LoggerAccessor
    {
        /// <summary>
        /// LoggerError
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logType"></param>
        public delegate void LoggerErrorAction(Exception ex, LogType logType = LogType.ServerLog);
        /// <summary>
        /// LoggerMonitor
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="logType"></param>
        public delegate void LoggerMonitorAction(string msg, LogType logType = LogType.ServerLog);

        private static Lazy<LoggerAccessor> instance = new Lazy<LoggerAccessor>(() => new LoggerAccessor(), true);

        /// <summary>
        /// Instance
        /// </summary>
        public static LoggerAccessor Instance
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
