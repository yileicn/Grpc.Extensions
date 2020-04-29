using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProtoBuf;

namespace Grpc.Extension.Common
{
    /// <summary>
    /// ProtobufExtensions
    /// </summary>
    public class ProtobufExtensions
    {
        /// <summary>
        /// 序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] Serialize<T>(T input)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                Serializer.Serialize<T>((Stream)memoryStream, input);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] data)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
                return Serializer.Deserialize<T>((Stream)memoryStream);
        }
    }
}
