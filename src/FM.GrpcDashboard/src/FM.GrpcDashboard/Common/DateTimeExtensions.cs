namespace System
{
    public static class DateTimeExtensions
    {
        public static DateTime FromUnixTimestamp(this long unixtime)
        {
            //DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime sTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Utc, TimeZoneInfo.Local);
            return sTime.AddMilliseconds(unixtime);
        }

        public static long ToUnixTimestamp(this DateTime datetime)
        {
            //DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime sTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Utc, TimeZoneInfo.Local);
            return (long)(datetime - sTime).TotalMilliseconds;
        }
    }
}
