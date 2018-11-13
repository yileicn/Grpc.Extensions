using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grpc.Extension.Internal
{
    internal class ThrottleManager
    {
        private static readonly object sync = new object();
        private List<string> throttleMethods = new List<string>();
        private static Lazy<ThrottleManager> instance = new Lazy<ThrottleManager>(() => new ThrottleManager(), true);
        public static ThrottleManager Instance => instance.Value;

        public void Add(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return;
            lock (sync)
            {
                throttleMethods.Add(fullName.Trim());
                throttleMethods = throttleMethods.Distinct().ToList();
            }
        }

        public void Del(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return;
            lock (sync)
            {
                throttleMethods.Remove(fullName.Trim());
            }
        }

        public bool IsThrottled(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return false;
            lock (sync)
            {
                return throttleMethods.Contains(fullName.Trim());
            }
        }
    }
}
