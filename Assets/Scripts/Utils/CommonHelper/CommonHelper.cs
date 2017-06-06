/*
 * 公共方法工具类
 * */

using System;

public class CommonHelper
{
    public const long begin = 621355968000000000; //1970,1,1到公元元年的时间差

    /// <summary>
    /// 当前机器的UTC时间(毫秒)
    /// </summary>
    public static long _UtcNowMS
    {
        get
        {
            return (DateTime.Now.ToUniversalTime().Ticks - begin) / 10000;
        }
    }

    /// <summary>
    /// 当前机器时间的UTC时间(纳秒)
    /// </summary>
    public static long _UtcNowS
    {
        get
        {
            return (DateTime.UtcNow.Ticks - begin) / 10000000;
        }
    }

    /// <summary>
    /// UTC时间(毫秒)转化为DateTime
    /// </summary>
    public static DateTime UtcMsToDateTime(long utcMillisecond)
    {
        return new DateTime(utcMillisecond * 10000 + begin - DateTime.UtcNow.Ticks + DateTime.Now.Ticks);
    }

    /// <summary>
    /// UTC时间(秒)转化为DateTime
    /// </summary>
    /// <param name="utcSecond"></param>
    /// <returns></returns>
    public static DateTime UtcSToDateTime(long utcSecond)
    {
        return UtcMsToDateTime(utcSecond * 1000);
    }

    /// <summary>
    /// 时间间隔(毫秒)
    /// </summary>
    /// <returns></returns>
    public static TimeSpan CalculateMsToTimeSpan(long utcMsSmall, long utcMsLarge)
    {
        TimeSpan ts = TimeSpan.FromMilliseconds(utcMsLarge - utcMsSmall);

        return ts;
    }

    /// <summary>
    /// 时间间隔（秒）
    /// </summary>
    /// <param name="utcSecondSmall"></param>
    /// <param name="utcSecondLarge"></param>
    /// <returns></returns>
    public static TimeSpan CalculateSToTimeSpan(long utcSecondSmall, long utcSecondLarge)
    {
        TimeSpan ts = TimeSpan.FromSeconds(utcSecondLarge - utcSecondSmall);

        return ts;
    }
}
