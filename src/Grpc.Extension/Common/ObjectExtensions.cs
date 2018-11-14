using System;
using System.Linq;
using System.Reflection;

namespace Grpc.Extension.Common
{
    internal static class ObjectExtensions
    {
        public static object FillProp(this object src)
        {
#if NET45
            foreach (var p in src.GetType().GetProperties())
            {
                try
                {
                    if (p.PropertyType.IsPrimitive ||
                        p.PropertyType.Equals(typeof(string)) ||
                        p.PropertyType.Equals(typeof(DateTime)) ||
                        p.PropertyType.Equals(typeof(Decimal)) ||
                        p.PropertyType.Equals(typeof(Guid)) ||
                        p.PropertyType.Equals(typeof(DateTimeOffset)) ||
                        p.PropertyType.Equals(typeof(TimeSpan)))
                    {
                        continue;
                    }

                    if (GrpcExtensionsOptions.Instance.FillPropExcludePrefixs.Any(q => p.PropertyType.FullName.StartsWith(q)))
                    {
                        continue;
                    }

                    var subSrc = Activator.CreateInstance(p.PropertyType);
                    subSrc = FillProp(subSrc);
                    p.SetValue(src, subSrc);
                }
                catch
                {
                    continue;
                }
            }
            return src;
#endif

#if NETSTANDARD2_0
            foreach (var p in src.GetType().GetTypeInfo().GetProperties())
            {
                try
                {
                    if (p.PropertyType.GetTypeInfo().IsPrimitive ||
                        p.PropertyType.GetTypeInfo().Equals(typeof(string)) ||
                        p.PropertyType.GetTypeInfo().Equals(typeof(DateTime)) ||
                        p.PropertyType.GetTypeInfo().Equals(typeof(Decimal)) ||
                        p.PropertyType.GetTypeInfo().Equals(typeof(Guid)) ||
                        p.PropertyType.GetTypeInfo().Equals(typeof(DateTimeOffset)) ||
                        p.PropertyType.GetTypeInfo().Equals(typeof(TimeSpan)))
                    {
                        continue;
                    }

                    if (GrpcExtensionsOptions.Instance.FillPropExcludePrefixs.Any(q => p.PropertyType.GetTypeInfo().FullName.StartsWith(q)))
                    {
                        continue;
                    }

                    var subSrc = Activator.CreateInstance(p.PropertyType);
                    subSrc = FillProp(subSrc);
                    p.SetValue(src, subSrc);
                }
                catch
                {
                    continue;
                }
            }
            return src;
#endif
        }
    }
}
