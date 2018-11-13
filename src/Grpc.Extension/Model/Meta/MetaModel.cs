using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.Model
{
    public class MetaModel
    {
        public static string Ip { get; set; }

        public static int Port { get; set; }

        public static DateTime StartTime { get; set; }

        public static List<MetaMethodModel> Methods { get; set; } = new List<MetaMethodModel>();
    }

    public class MetaMethodModel
    {
        public string FullName { get; set; }

        public Type RequestType { get; set; }

        public Type ResponseType { get; set; }

        public Delegate Handler { get; set; }
    }
}
