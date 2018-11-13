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
            var callHandlers = chReflector.GetValue(obj) as T;

            return callHandlers;
        }

        public static Tuple<T, FieldInfo> GetFieldValue<T>(this object obj, string name, BindingFlags bindingFlags) where T : class
        {
            var chField = obj.GetType().GetTypeInfo().GetField(name, bindingFlags);
            var chReflector = chField.GetReflector();
            var callHandlers = chReflector.GetValue(obj) as T;

            return Tuple.Create(callHandlers, chField);
        }
    }
}
