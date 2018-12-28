/// 每个页面都含有自己单独的optionString;
/// 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UI
{
    /// <summary>
    /// 界面UI的划分，1.全屏页面：0-499；2.覆盖式页面：500-999；3.弹出窗面板：1000-1499；4.引导面板（1500-1999）.5.退出面板（2000+）（程序）
    /// </summary>
    public class Page : MonoBehaviour,IHidable
	{
        public PageType pageType = PageType.FULL_SCREEN;

        public Texture2D[] atlasOfPage;
        public GameObject[] redTips;

        /// <summary>
        /// 是否显示顶部条，金币体力等
        /// </summary>
        //public bool showTopBar = false;

        /// <summary>
        /// 是否永久驻留在内存
        /// </summary>
        public bool alwaysInMemery = true;

        /// <summary>
        /// 保持在历史里
        /// </summary>
        public bool saveToHistory = true;

        /// <summary>
        /// 是否显示摇杆
        /// </summary>
        public bool showEasyTouch = false;

        public bool showScene = false;
        /// <summary>
        /// 页面实现的初始位置
        /// </summary>
        public Vector3 initPosition = Vector3.zero;
        public PageManager pageManager;
		//操作字符串
		protected string optionString;
		public string OptionString
		{
			get{ return optionString;}
			set{ optionString = value;}
		}
		// 存放解析字符串
		public Dictionary<string,string> options = new Dictionary<string, string> ();

		// 页面是否被打开
		public bool isOpen=false;
		// 重新设置操作字符串
		public string GetOptString()
		{
            SaveOptString();
            optionString = "";
			foreach( KeyValuePair<string,string> opt in options )
			{
				optionString += opt.Key + "=" + opt.Value +"&" ;
			}
            return optionString;
		}

        //将显示逻辑 转化成字符串键值对
        protected virtual void SaveOptString()
        { 
            
        }
		// 解析操作字符串
		protected void ParseOptString()	//逻辑
		{
            options.Clear();
			string[] strArr = optionString.Split(new char[1]{'&'},System.StringSplitOptions.RemoveEmptyEntries);
			foreach(string s in strArr)
			{
				if(s.Contains("="))
				{
					string[] strV = s.Split(new char[1]{'='});
                   
                    if(options.ContainsKey(strV[0]))
                    {
                        options[strV[0]] = strV[1];
                    }
                    else
                    {
                        options.Add(strV[0], strV[1]);
                    }
                  
				}
			}
		}

        public void AddOptionString(string optString) {
			string[] strArr = optString.Split(new char[1]{'&'},System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in strArr) {
                if (s.Contains("s")) {
					string[] strV = s.Split(new char[1]{'='});
                    if (options.ContainsKey(strV[0]))
                    {
                        options[strV[0]] = strV[1];
                    }
                    else {
                        options.Add(strV[0], strV[1]);
                    }
                }
            }
        }

        //打开一个页面
        public void Open(string options)
        {
            this.optionString = options;
            isOpen = true;
            ParseOptString();
            Show();
            DoOpen();
        }

        //有子类中覆盖的打开方法，进行字符串键值对转化成显示逻辑
        protected virtual void DoOpen()
        {
        }

        public virtual void OnForceClose()
        { 
            
        }

        //关闭页面，同时通知PageManager关闭当前页面
        public virtual void Close()
        {
            isOpen = false;
            Hide();
            
            DoClose();
            PageManager.Instance.OnClosePage(this);
        }

        public virtual void Reopen(string options)
        {
            this.optionString = options;
            ParseOptString();
            Debug.Log("reopen");
            DoReopen();
        }

        public void CorverdForceClose()
        {
            //if(pageType == PageType.COVER)
            //{
                DoClose();
            //}
        }

        public virtual void DoReopen()
        { 
            
        }

        //有子类中覆盖的关闭方法，可以进行一些特定操作
        protected virtual void DoClose()
        {
            
        }

        //设置页面某一参数的值
        public virtual void SetOptionValue(string key,string value)
        {
            options[key] = value;
        }

        //隐藏当前页面
        public virtual void Hide()
        {
            //transform.localPosition = new Vector3(-10000f, 10000f, 10000f);
            gameObject.SetActive(false);
            isOpen = false;
        }

        //显示当前页面
        public virtual void Show()
        {
            transform.localPosition = initPosition;
            gameObject.SetActive(true);
            isOpen = true;
        }
        
        public virtual void OnCoverPageRemove()
        {
        	Debug.Log("Cover page remove");
        
        }
		public virtual void OnMemoryPageDestory()
		{
			Debug.Log("Destory from memory!");
		}

        public virtual void OnCoverPageOpen()
        {
            Debug.Log("Cover page open");
        }

        /// <summary>
        /// 网络断开连接
        /// </summary>
        public virtual void OnDisconnect()
        {

        }

        /// <summary>
        /// 网络重新连接
        /// </summary>
        public virtual void OnReconnect()
        {

        }
	}
}