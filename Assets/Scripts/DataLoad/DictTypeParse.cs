/************************************************* 
Copyright: 
Author: 
Date:
Description:数据表类型解析
**************************************************/
using System;
using System.Collections.Generic;
using Util;

namespace DataLoad
{
    /// <summary>
    /// 支持类型int、float、String、Array
    /// 此类可以后续可以扩展
    /// </summary>
    public class DictTypeParse
    {
        private static readonly string[] m_splitArray = new string[] { "&&" };
        private static readonly string[] m_splitMap = new string[] { ":" };

        public static int ToInt(string str)
        {
            int value = 0;
            if (!int.TryParse(str, out value))
            {
                LoggerHelper.Error(string.Format("ToInt Error! str: {0} is error", str));
            }


            return value;
        }

        public static float ToFloat(string str)
        {
            float value = 0f;
            if (!float.TryParse(str, out value))
            {
                LoggerHelper.Error(string.Format("ToFloat Error! str: {0} is error", str));
            }

            return value;
        }

        public static string ToString(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                return str;
            }

            return "";
        }

        public static string[] SplitString2Array(string str)
        {
            string[] arrayStr = str.Split(m_splitArray, StringSplitOptions.None);

            return arrayStr;
        }

        public static List<int> ToIntArray(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                string[] arrayStr = SplitString2Array(str);
                List<int> list = new List<int>();
                for(int i=0; i != arrayStr.Length; ++i)
                {
                    list.Add(ToInt(arrayStr[i]));
                }

                return list;
            }

            return null;
        }

        public static List<float> ToFloatArray(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                string[] arrayStr = SplitString2Array(str);
                List<float> list = new List<float>();
                for (int i = 0; i != arrayStr.Length; ++i)
                {
                    list.Add(ToFloat(arrayStr[i]));
                }

                return list;
            }

            return null;
        }

        public static List<string> ToStringArray(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                string[] arrayStr = SplitString2Array(str);
                List<string> list = new List<string>();
                for (int i = 0; i != arrayStr.Length; ++i)
                {
                    list.Add(arrayStr[i]);
                }

                return list;
            }

            return null;
        }
    }
}
