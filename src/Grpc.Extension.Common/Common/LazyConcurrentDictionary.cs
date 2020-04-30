using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Grpc.Extension.Common
{
    /// <summary>
    /// LazyConcurrentDictionary(Thread Safe)
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class LazyConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, Lazy<TValue>>
    {

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            return base.GetOrAdd(key, k => new Lazy<TValue>(() => valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return base.AddOrUpdate(key, k => new Lazy<TValue>(() => addValueFactory(k), true),
                (k, v) => new Lazy<TValue>(() => updateValueFactory(k, v.Value), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }
    }
}
