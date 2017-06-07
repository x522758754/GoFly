/************************************************* 
Copyright: 
Author: auto-generate
Date:
Description:数据表管理类 part2
**************************************************/

namespace DataLoad
{
    public partial class DictManager
    {
        #region 所有表名
        
        /// <summary>
        /// 所有表名
        /// </summary>
        public string[] _ArrayDictNames = {"", };

        #endregion

        #region 声明 DictLoader

        public TotalDictsDictLoader _TotalDicts = new TotalDictsDictLoader();

        #endregion

        #region 加载数据

        /// <summary>
        /// 加载数据
        /// </summary>
        private void Loads()
        {
            _TotalDicts.LoadDictFile(_DirPath);
        }

        #endregion

        #region 卸载数据 

        /// <summary>
        /// 卸载数据
        /// </summary>
        private void Unloads()
        {
            _TotalDicts.Release();
        }

        #endregion
    }
}
