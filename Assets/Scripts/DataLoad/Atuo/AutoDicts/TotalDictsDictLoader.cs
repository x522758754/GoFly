

namespace DataLoad
{
    [System.Serializable]
    public class TotalDictsModel:DictModel
    {
        public int id;
        public string tableName;
        public int bPreloading;
    }

    public class TotalDictsDictLoader : DictLoader
    {
        /// <summary>
        /// 获得表名
        /// </summary>
        /// <returns></returns>
        protected override string GetFileName()
        {
            return "total_dicts";
        }

        /// <summary>
        /// 解析行数据 => T
        /// </summary>
        /// <param name="rowData">每一行的数据</param>
        /// <returns></returns>
        protected override DictModel ParseRowData(string[] rowData)
        {
            TotalDictsModel model = new TotalDictsModel();
            model.id = DictTypeParse.ToInt(rowData[0]);
            model.tableName = DictTypeParse.ToString(rowData[1]);
            model.bPreloading = DictTypeParse.ToInt(rowData[2]);

            return model;
        }

    }
}