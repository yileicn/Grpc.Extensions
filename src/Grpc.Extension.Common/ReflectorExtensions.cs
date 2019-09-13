using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AspectCore.Extensions.Reflection;

namespace Grpc.Extension.Common
{
    /// <summary>
    /// ReflectorExtensions
    /// </summary>
    public static class ReflectorExtensions
    {
        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="bindingFlags"></param>
        /// <returns></returns>
        public static T GetPropertyValue<T>(this object obj, string name, BindingFlags bindingFlags) where T : class
        {
            var chProperty = obj.GetType().GetTypeInfo().GetProperty(name, bindingFlags);
            if (chProperty == null) throw new InvalidOperationException($"Cannot locate property {name}");
            var chReflector = chProperty.GetReflector();
            var value = chReflector.GetValue(obj) as T;

            return value;
        }

        /// <summary>
        /// 获取字段值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        /// <param name="bindingFlags"></param>
        /// <returns></returns>
        public static Tuple<T, FieldInfo> GetFieldValue<T>(this object obj, string name, BindingFlags bindingFlags) where T : class
        {
            var chField = obj.GetType().GetTypeInfo().GetField(name, bindingFlags);
            if (chField == null) throw new InvalidOperationException($"Cannot locate field {name}");
            var chReflector = chField.GetReflector();
            var value = chReflector.GetValue(obj) as T;

            return Tuple.Create(value, chField);
        }

        /// <summary>
        /// 获取字段值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="bindingFlags"></param>
        /// <returns></returns>
        public static T GetFieldValue<T>(this Type type, string name, BindingFlags bindingFlags) where T : class
        {
            var chField = type.GetTypeInfo().GetField(name, bindingFlags);
            if (chField == null) throw new InvalidOperationException($"Cannot locate field {name}");
            var chReflector = chField.GetReflector();
            var value = chReflector.GetValue(null) as T;

            return value;
        }

        /// <summary>
        /// 获取方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="bindingFlags"></param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo(this Type type, string name, BindingFlags bindingFlags)
        {
            var method = type.GetMethod(name, bindingFlags);
            if(method == null) throw new InvalidOperationException($"Cannot locate method {name}");
            return method;
        }
    }
}
