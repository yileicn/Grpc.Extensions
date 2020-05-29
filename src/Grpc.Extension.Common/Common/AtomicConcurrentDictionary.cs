using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Grpc.Extension.Common
{
    /// <summary>
    /// AtomicConcurrentDictionary(Thread Safe)
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class AtomicConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
    {
        private static readonly LazyConcurrentDictionary<int, SemaphoreSlim> _semaphores = new LazyConcurrentDictionary<int, SemaphoreSlim>();
        public async Task<TValue> GetOrAddAsync(TKey key, Func<TKey, Task<TValue>> valueFactory)
        {
            if (base.TryGetValue(key, out var value))
                return value;

            var semaphore = GetSemaphore(key);

            await semaphore.WaitAsync()
                .ConfigureAwait(false);
            try
            {
                if (!base.TryGetValue(key, out value))
                {
                    value = await valueFactory(key);
                    return base.GetOrAdd(key, value);
                }
                return value;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private SemaphoreSlim GetSemaphore(TKey key)
        {
            var semaphoreKey = key.GetHashCode();
            return _semaphores.GetOrAdd(semaphoreKey, k => new SemaphoreSlim(1));
        }
    }
}
