using System;
using System.Collections.Generic;
using System.Text;

namespace Grpc.Extension.Common
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// 返回一个FlatException
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetFlatException(this Exception ex)
        {
            var exception = "";
            if (ex is AggregateException aex)
            {
                foreach (var e in aex.Flatten().InnerExceptions)
                {
                    exception += e?.ToString() + Environment.NewLine;
                }
            }
            else
            {
                exception = ex.ToString();
            }
            return exception;
        }
    }
}
