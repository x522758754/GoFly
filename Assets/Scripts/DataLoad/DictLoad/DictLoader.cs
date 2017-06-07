/************************************************* 
Copyright: 
Author: 
Date:
Description:数据表加载类 
**************************************************/

using System;
using System.Collections.Generic;
using Util;

namespace DataLoad
{
    /// <summary>
    /// 备注：此类目前缺陷 在于查找不方便，需遍历，后续可以考虑
    /// 可以维护一张词典Dict缓存
    /// 或List接口
    /// </summary>
    public abstract class DictLoader
    { 
        private List<DictModel> m_listCache;

        /// <summary>
        /// T 数据缓存列表
        /// </summary>
        public List<DictModel> _ListCache { get { return m_listCache; } }

        /// <summary>
        /// 获得表名
        /// </summary>
        /// <returns></returns>
        protected abstract string GetFileName();

        /// <summary>
        /// 解析行数据 => T
        /// </summary>
        /// <param name="rowData">每一行的数据</param>
        /// <returns></returns>
        protected abstract DictModel ParseRowData(string[] rowData);

        /// <summary>
        /// 加载数据文件txt
        /// </summary>
        /// <param name="filePath"></param>
        public void LoadDictFile(string dirPath)
        {
            string fileName = GetFileName();
            DictFileReader dfr = new DictFileReader(string.Format("{0}/{1}", dirPath, fileName));

            bool bError = false;
            m_listCache = new List<DictModel>();
            string[] datas;
            do
            {
                datas = dfr.ReadRow();
                if(null != datas)
                {
                    DictModel t = ParseRowData(datas);
                    if (null != t)
                    {
                        m_listCache.Add(t);
                    }
                    else
                    {
                        bError = true;
                        break;
                    }
                }
            }
            while (null != datas);
            dfr.Close();

            if(bError)
            {
                LoggerHelper.Error(string.Format("{0} LoadDictFile Error!", dirPath));
            }
        }

        public void Release()
        {
            if(null != m_listCache)
            {
                m_listCache.Clear();

                m_listCache = null;
            }
        }
    }
}
