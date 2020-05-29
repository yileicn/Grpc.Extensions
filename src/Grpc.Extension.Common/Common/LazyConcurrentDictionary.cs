using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Grpc.Extension.Common
{
    /// <summary>
    /// LazyConcurrentDictionary(Thread Safe)
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class LazyConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, AsyncLazy<TValue>>
    {
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            var data = base.GetOrAdd(key, k => new AsyncLazy<TValue>(() => valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
            return data.Result;
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            var data = base.AddOrUpdate(key, k => new AsyncLazy<TValue>(() => addValueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication),
                (k, v) => new AsyncLazy<TValue>(() => updateValueFactory(k, v.Value.Result), LazyThreadSafetyMode.ExecutionAndPublication)).Value;

            return data.Result;
        }

        public Task<TValue> GetOrAddAsync(TKey key, Func<TKey, Task<TValue>> valueFactory)
        {
            return base.GetOrAdd(key, k => new AsyncLazy<TValue>(async () => await valueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        public Task<TValue> AddOrUpdateAsync(TKey key, Func<TKey, Task<TValue>> addValueFactory, Func<TKey, Task<TValue>, Task<TValue>> updateValueFactory)
        {
            return base.AddOrUpdate(key, k => new AsyncLazy<TValue>(async () => await addValueFactory(k), LazyThreadSafetyMode.ExecutionAndPublication),
                (k, v) => new AsyncLazy<TValue>(async () => await updateValueFactory(k, v.Value), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }
    }

    public class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory, LazyThreadSafetyMode mode) :
            base(() => Task.Factory.StartNew(valueFactory), mode)
        { }
        public AsyncLazy(Func<Task<T>> taskFactory, LazyThreadSafetyMode mode) :
            base(() => Task.Factory.StartNew(() => taskFactory()).Unwrap(), mode)
        { }
    }
}
