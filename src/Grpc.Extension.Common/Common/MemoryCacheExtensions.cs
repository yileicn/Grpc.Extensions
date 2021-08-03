using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grpc.Extension.Common
{
    /// <summary>
    /// MemoryCacheExtensions(Thread Safe)
    /// </summary>
    public static class MemoryCacheExtensions
    {
        private static readonly LazyConcurrentDictionary<int, SemaphoreSlim> _semaphores = new LazyConcurrentDictionary<int, SemaphoreSlim>();

        public static async Task<T> GetOrCreateAtomicAsync<T>(this IMemoryCache memoryCache, object key, Func<ICacheEntry, Task<T>> factory)
        {
            if (memoryCache.TryGetValue(key, out var value))
                return (T)value;

            var semaphoreKey = (memoryCache, key).GetHashCode();
            var semaphore = GetSemaphore(semaphoreKey);

            await semaphore.WaitAsync()
                           .ConfigureAwait(false);
            try
            {
                if (!memoryCache.TryGetValue(key, out value))
                {
                    var entry = memoryCache.CreateEntry(key);
                    value = await factory(entry);
                    entry.SetValue(value);
                    entry.Dispose();
                    return (T)value;
                }

                return (T)value;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static SemaphoreSlim GetSemaphore(int semaphoreKey)
        {
            return _semaphores.GetOrAdd(semaphoreKey, k => new SemaphoreSlim(1));
        }
    }
}
