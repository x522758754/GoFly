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
    public partial class DictManager
    {
        #region 备注所有表名
        
        /// <summary>
        /// 所有表名
        /// </summary>
        public string[] _ArrayDictNames = {"", };

        #endregion

        #region 声明 DictLoader

        public TotalDictsDictLoader _TotalDicts = new TotalDictsDictLoader();

        #endregion

        #region 加载数据

        private void Loads()
        {
            _TotalDicts.LoadDictFile(_DirPath);
        }

        #endregion

        #region 卸载数据 

        private void Unloads()
        {
            _TotalDicts.Release();
        }

        #endregion
    }
}
