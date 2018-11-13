using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AspectCore.Extensions.Reflection;

namespace Grpc.Extension.Common
{
    public static class ReflectorExtensions
    {
        public static T GetPropertyValue<T>(this object obj, string name, BindingFlags bindingFlags) where T : class
        {
            var chProperty = obj.GetType().GetTypeInfo().GetProperty(name, bindingFlags);
            var chReflector = chProperty.GetReflector();
            var value = chReflector.GetValue(obj) as T;

            return value;
        }

        public static Tuple<T, FieldInfo> GetFieldValue<T>(this object obj, string name, BindingFlags bindingFlags) where T : class
        {
            var chField = obj.GetType().GetTypeInfo().GetField(name, bindingFlags);
            var chReflector = chField.GetReflector();
            var value = chReflector.GetValue(obj) as T;

            return Tuple.Create(value, chField);
        }

        public static T GetFieldValue<T>(this Type type, string name, BindingFlags bindingFlags) where T : class
        {
            var chField = type.GetTypeInfo().GetField(name, bindingFlags);
            var chReflector = chField.GetReflector();
            var value = chReflector.GetValue(null) as T;

            return value;
        }
    }
}
