using System;
using System.Collections.Generic;
using UnityEngine;
using Util;

namespace UI
{
    /// <summary>
    /// 页面类型
    /// </summary>
    public enum PageType : byte
    {
        /// <summary>
        /// 全屏页面
        /// </summary>
        FULL_SCREEN,

        /// <summary>
        /// 覆盖页面
        /// </summary>
        COVER_SCREEN,
        COVER
    }

    public enum PageCloseWay : byte
    {
        /// <summary>
        /// 销毁
        /// </summary>
        ///Destroy,

        /// <summary>
        /// 不可见
        /// </summary>
        InActive,

        /// <summary>
        /// 移到屏幕外
        /// </summary>
        MoveToScreen
    }

    public class PageInfo
    {
        public string m_pageName;
        public PageType m_pageType;
        public bool m_bAlwaysInMemery;
        public bool m_bSaveToHistory;
        public string m_strOption;
    }
    public class UIPage : MonoBehaviour
    {
        /// <summary>
        /// 当前界面使用的图集
        /// </summary>
        public Texture2D[] m_atlasOfPage;

        /// <summary>
        /// 页面类型
        /// </summary>
        public PageType m_pageType = PageType.FULL_SCREEN;

        /// <summary>
        /// 页面关闭的处理方式
        /// </summary>
        public PageCloseWay m_pageCloseWay = PageCloseWay.InActive;

        /// <summary>
        /// 是否永驻内存
        /// </summary>
        public bool m_bAlwaysInMemery = false;

        /// <summary>
        /// 是否保存在历史记录
        /// </summary>
        public bool m_bSaveToHistory = true;

        /// <summary>
        /// 界面是否被打开
        /// </summary>
        protected bool m_bIsOpen = false;

        private readonly char[] m_strOptionSplitKey = new char[1] { '&' };
        private readonly char[] m_strOptionSplitValue = new char[1] { '=' };

        /// <summary>
        /// 选项字符串
        /// </summary>
        protected string m_strOption;

        /// <summary>
        /// 存储解析字符
        /// </summary>
        protected Dictionary<string, string> m_dictOptions = new Dictionary<string, string>();

        private readonly Vector3 m_initPosition = Vector3.zero;
        private readonly Vector3 m_moveToPosition = new Vector3(10000f, 10000f, 10000f);

        /// <summary>
        /// 解析Option String（Key,Value）
        /// </summary>
        protected void ParseOptionString()
        {
            m_dictOptions.Clear();
            string[] strArray = m_strOption.Split(m_strOptionSplitKey, StringSplitOptions.RemoveEmptyEntries);
            for(int i=0; i != strArray.Length; ++i)
            {
                if(strArray[i].Contains(m_strOptionSplitValue.ToString()))
                {
                    string[] strKeyValue = strArray[i].Split(m_strOptionSplitValue);
                    if(m_dictOptions.ContainsKey(strKeyValue[0]))
                    {
                        m_dictOptions[strKeyValue[0]] = strKeyValue[1];
                    }
                    else
                    {
                        m_dictOptions.Add(strKeyValue[0], strKeyValue[1]);
                    }
                }
            }
        }

        /// <summary>
        /// 得到Option String Value by Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string GetOptionStringValue(string key)
        {
            string value = null;
            if (m_dictOptions.ContainsKey(key))
                value = m_dictOptions[key];

            return value;
        }

        /// <summary>
        /// 返回 Option String
        /// </summary>
        /// <returns></returns>
        protected string GetOptionString()
        {
            return m_strOption;
        }

        /// <summary>
        /// 打开页面
        /// </summary>
        /// <param name="option"></param>
        public void Open(string option)
        {
            m_strOption = option;
            ParseOptionString();
            DoOpen();

            Show();
            m_bIsOpen = true;
        }

        /// <summary>
        /// 关闭页面，通知UIPageManager关闭页面
        /// </summary>
        public void Close()
        {
            m_bIsOpen = false;

            DoClose();

            //通知PageManager关闭页面
            UIPageManager.Instance.OnClosePage(this);
        }

        /// <summary>
        /// 重新打开页面
        /// </summary>
        /// <param name="option"></param>
        public void Reopen(string option)
        {
            LoggerHelper.Info("Reopen");
            m_strOption = option;
            ParseOptionString();
            DoOpen();
        }

        /// <summary>
        /// 强制移开页面上的Cover页面
        /// </summary>
        public void CorverdForceClose()
        {
            DoClose();
        }

        /// <summary>
        /// 显示页面
        /// </summary>
        public virtual void Show()
        {
            //事件统一传参处理
            transform.localPosition = m_initPosition;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 隐藏页面
        /// </summary>
        public virtual void Hide()
        {
            switch(m_pageCloseWay)
            {
                case PageCloseWay.InActive:
                    this.gameObject.SetActive(false);
                    break;
                case PageCloseWay.MoveToScreen:
                    this.transform.position = m_moveToPosition;
                    break;
                default:
                    LoggerHelper.Error(string.Format("PageCloseWay: {0} is error!", m_pageCloseWay));
                    break;
            }
        }

        /// <summary>
        /// 页面被打开时调用
        /// </summary>
        protected virtual void DoOpen()
        {

        }

        /// <summary>
        /// 页面被关闭时调用
        /// </summary>
        protected virtual void DoClose()
        {

        }

        /// <summary>
        /// 当页面上的Cover Page被关闭调用
        /// </summary>
        public virtual void OnCoverPageClose()
        {
            LoggerHelper.Info("Cover page remove");

        }

        /// <summary>
        /// 当页面有Cover Page被打开时调用
        /// </summary>
        public virtual void OnCoverPageOpen()
        {
            LoggerHelper.Info("Cover page open");
        }

        /// <summary>
        /// 当前页面被强制关闭时调用
        /// </summary>
        public virtual void OnForceClose()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void OnMemoryPageDestory()
        {
            LoggerHelper.Info("Destory from memory!");
        }

        public PageInfo GetPageInfo()
        {
            PageInfo info = new PageInfo();

            info.m_bAlwaysInMemery = m_bAlwaysInMemery;
            info.m_bSaveToHistory = m_bSaveToHistory;
            info.m_pageName = this.name;
            info.m_pageType = m_pageType;
            info.m_strOption = m_strOption;

            return info;    
        }

        public void SetPageInfo(PageInfo info)
        {
            m_bAlwaysInMemery = info.m_bAlwaysInMemery;
            m_bSaveToHistory = info.m_bSaveToHistory;
            m_pageType = info.m_pageType;
            m_strOption = info.m_strOption;
        }

        public bool IsOpen()
        {
            return m_bIsOpen;
        }
    }
}
