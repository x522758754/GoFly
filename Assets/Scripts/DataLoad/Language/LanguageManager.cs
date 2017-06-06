/*
 * 语言表：json解析
 * */

using LitJson;
using Util;

public class LanguageManager : Singleton<LanguageManager>
{
    private JsonData m_jsonData;

    public void Load(string path)
    {
        m_jsonData = JsonTool.LoadPath(path);
    }

    public string GetText(string moduleName, string keyName)
    {
        string str = string.Empty;
        try
        {
            str =  m_jsonData[moduleName][keyName].ToString();
        }
        catch(System.Exception ex)
        {
            LoggerHelper.Except(ex);
        }

        return string.Empty;
    }
}
