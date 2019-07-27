using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Extension.Model;

namespace Grpc.Extension.Internal
{
    internal class MonitorManager
    {
        private static readonly object syncSaveResponseMethods = new object();
        private static List<string> saveResponseMethods = new List<string>();
        private static readonly Lazy<MonitorManager> instance = new Lazy<MonitorManager>(() => new MonitorManager(), true);

        internal static MonitorManager Instance
        {
            get { return instance.Value; }
        }

        #region 是否记录响应数据到日志
        public void AddSaveResponseMethod(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return;
            lock (syncSaveResponseMethods)
            {
                saveResponseMethods.Add(fullName);
                saveResponseMethods = saveResponseMethods.Distinct().ToList();
            }
        }

        public void DelSaveResponseMethod(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return;
            lock (syncSaveResponseMethods)
            {
                saveResponseMethods.Remove(fullName);
            }
        }

        public bool SaveResponseMethodEnable(string fullName)
        {
            return GrpcExtensionsOptions.Instance.GlobalSaveResponseEnable || (!string.IsNullOrWhiteSpace(fullName) && saveResponseMethods.Contains(fullName));
        }
        #endregion
    }
}
