/************************************************* 
Copyright: 
Author: 
Date:
Description:数据表IO读取 
**************************************************/
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace DataLoad
{
    /// <summary>
    /// 解析表
    /// </summary>
    public class DictFileReader
    {
        private readonly char splitChars = '\t';

        private StreamReader m_reader;
        private MemoryStream m_memoryStream;

        public DictFileReader(string filePath)
        {
            //bundle或Resource.Load
            //先用Resource.Load，后续需要有一个资源加载类
            TextAsset text = Resources.Load(filePath) as TextAsset;
            if (null != text)
            {
                m_memoryStream = new MemoryStream(text.bytes);
                m_reader = new StreamReader(m_memoryStream);
            }
            else
            {
                
            }
        }

        /// <summary>
        /// 读取行数据
        /// </summary>
        /// <returns></returns>
        public string[] ReadRow()
        {
            string line = m_reader.ReadLine();
            if (line == null)
            {
                return null;
            }
            return line.Split(splitChars);
        }

        /// <summary>
        /// 关闭数据流
        /// </summary>
        public void Close()
        {
            if(null != m_reader)
            {
                m_reader.Close();
                m_reader = null;
            }
            if(null != m_memoryStream)
            {
                m_memoryStream.Close();
                m_memoryStream = null;
            }
        }

    }
}

