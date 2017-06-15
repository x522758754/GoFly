/*  公共方法工具类
 * 
 * */

using System;
using System.Text;

public class CommonHelper
{
    #region 时间处理

    public const long begin = 621355968000000000; //1970,1,1到公元元年的时间差

    /// <summary>
    /// 当前机器的UTC时间(毫秒)
    /// </summary>
    public static long _UtcNowMs
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

    /* 时间格式->string
    dt.ToString();//2005-11-5 13:21:25
    dt.ToLongDateString().ToString();//2005年11月5日
    yyyy 包括纪元的四位数的年份。
    MM 月份数字。一位数的月份有一个前导零。
    dd 月中的某一天。一位数的日期有一个前导零。 
    HH 24 小时制的小时。一位数的小时数有前导零。
    mm 分钟。一位数的分钟数有一个前导零。 
    ss 秒。一位数的秒数有一个前导零。 
    */

    public static string DataTimeToString(DateTime dt)
    {
        string str = string.Empty;

        str = dt.ToString("yyyy-MM-dd HH:mm:ss");

        return str;
    }

    #endregion

    #region 动画处理

    #endregion

    #region C#处理 编码、流、byte

    /* 编码介绍
     * GB2312   简体中文字符集，采用双字节编码
     * GBK      扩展了GB2312，包括非常用简体汉字、繁体字、日语及朝鲜汉字
     * BIG      繁体中文，流行于台湾、香港与澳门，采用双字节编码
     * GB18030  与GB 2312-1980完全兼容，与GBK基本兼容，支持GB 13000及Unicode的全部统一汉字，共收录汉字70244个
     * Unicode  涵盖所有的文字、符号，每个符号都有独一无二的编码，全世界通用，四字节存储。
     * UTF-8    Unicode的一种实现方式，变长的编码方式，可以使用1~4个字节表示一个符号，根据不同的符号而变化字节长度。
     * UTF-16   标准的Unicode成为UTF-16
     * */

    public byte[] GetBytes(Encoding srcEncoding, Encoding dstEncoding, byte[] bytes)
    {
        return Encoding.Convert(srcEncoding, dstEncoding, bytes);
    }

    /* C#字节流处理部分类
     * Buffer 基元类型数组的相关操作 
     * Array 数组操作 
     * System.BitConverter 将基础数据类型与字节数组相互转换 
     * MemoryStream 创建其支持存储区为内存的流 
     * BinaryReader 用特定的编码将基元数据类型读作二进制值 
     * BinaryWriter 以二进制形式将基元类型写入流，并支持用特定的编码写入字符串。
     * StreamReader 实现一个 System.IO.TextReader，使其以一种特定的编码从字节流中读取字符 
     * StreamWriter 实现一个 System.IO.TextWriter，使其以一种特定的编码向流中写入字符
     * */

    #endregion

}
