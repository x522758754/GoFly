using System.Text;
using UnityEngine;
using LitJson;
using DataLoad;
using Util;

public class JsonTool
{
    public static JsonData LoadPath(string path)
    {
        TextAsset textAsset = ResourceManager.Instance.LoadResourceBlock(path) as TextAsset;

        if(null != textAsset)
        {
            string strJson = Encoding.UTF8.GetString(textAsset.bytes);
            JsonData jsonData = JsonMapper.ToObject(strJson);

            ResourceManager.Instance.UnloadResource(path);

            return jsonData;
        }
        else
        {
            LoggerHelper.Error(string.Format("the path not exist."));

            return null;
        }
    }
}
