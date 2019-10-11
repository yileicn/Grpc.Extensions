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
        public event LoggerErrorAction LoggerError;

        /// <summary>
        /// 触发写异常日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logType"></param>
        public void OnLoggerError(Exception ex, LogType logType = LogType.ServerLog)
        {
            LoggerError?.Invoke(ex, logType);
        }

        /// <summary>
        /// 写监控日志
        /// </summary>
        public event LoggerMonitorAction LoggerMonitor;

        /// <summary>
        /// 触发写监控日志
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="logType"></param>
        public void OnLoggerMonitor(string msg, LogType logType = LogType.ServerLog)
        {
            LoggerMonitor?.Invoke(msg, logType);
        }
    }
}
