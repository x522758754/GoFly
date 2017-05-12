/************************************************* 
Copyright: 
Author: 
Date:
Description:数据表管理类 part1:手写
**************************************************/

using System;
using System.Collections.Generic;
using Util;

namespace DataLoad
{
    public partial class DictManager:Singleton<DictManager>
    {
        private Dictionary<string, DictLoader> m_dictLoader;
        public Dictionary<string, DictLoader> _dictLoader { get { return m_dictLoader; } }

        private string m_dirPath;
        public string _DirPath { get { return m_dirPath; } }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialze(string dirPath)
        {
            m_dirPath = dirPath;

            Loads();
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Release()
        {
            Unloads();

            m_instance = null;
        }

    }
}
