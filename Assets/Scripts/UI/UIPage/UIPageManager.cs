using System.Collections.Generic;
using UnityEngine;
using Util;

namespace UI
{
    public class UIPageManager : Singleton<UIPageManager>
    {
        /// <summary>
        /// 当前页面
        /// </summary>
        public UIPage m_currentPage;

        /// <summary>
        /// 所有页面的父节点
        /// </summary>
        public GameObject m_goUIRoot;

        /// <summary>
        /// 历史记录中的页面列表;
        /// 历史记录的页面将被重新打开
        /// </summary>
        public Stack<PageInfo> m_stackPageHistory = new Stack<PageInfo>();

        /// <summary>
        /// 保存到内存中的页面列表;
        /// 页面将不会被销毁
        /// </summary>
        public Dictionary<string, UIPage> m_dictPageMemory = new Dictionary<string, UIPage>();

        /// <summary>
        /// 当前正在显示的页面
        /// </summary>
        private Dictionary<string, UIPage> m_dictPageActive = new Dictionary<string, UIPage>();

        public void Initialze()
        {

        }

        protected void Destory()
        {
            m_dictPageActive.Clear();
            m_dictPageMemory.Clear();
            m_stackPageHistory.Clear();
            m_currentPage = null;
        }

        /// <summary>
        /// 打开页面
        /// </summary>
        /// <param name="pageName"></param>
        /// <param name="option"></param>
        public void OpenPage(string pageName, string option)
        {
            LoggerHelper.Info(string.Format("Open page: {0}, option: {1}", pageName, option));

            if (null != m_currentPage && string.Equals(m_currentPage.name, pageName))
            {
                RealOpenPage(m_currentPage, option);
            }
            else
            {
                UIPage pageToOpen = GetPage(pageName);
                if (null != pageToOpen)
                {
                    DoBeforeOpen(m_currentPage, pageToOpen);

                    RealOpenPage(pageToOpen, option);
                }
                else
                {
                    LoggerHelper.Error(string.Format("Cann't finf the page: {0}", pageName));
                }

            }
        }

        /// <summary>
        /// 当页面关闭调用
        /// </summary>
        /// <param name="pageToClose"></param>
        public void OnClosePage(UIPage pageToClose)
        {
            if (null != pageToClose)
            {
                if (pageToClose != m_currentPage)
                {
                    LoggerHelper.Warning(string.Format("Page Close Error! pageToClose: {0}, m_currentPage: {1}", pageToClose.name, m_currentPage.name));
                }
                else if(!m_dictPageActive.ContainsKey(pageToClose.name))
                {
                    LoggerHelper.Error(string.Format("m_currentPage: {0},pageToClose: {1}, topPage: page"));
                }
                else
                {
                    PageInfo pageInfo = HistoryStackTopPage();
                    UIPage pageToOpen = GetPage(pageInfo.m_pageName);
                    if (null != pageToOpen)
                    {
                        DoBeforeClose(pageToClose, pageToOpen);
                        RealClosePage(pageToClose);
                        RealOpenPage(pageToOpen, pageInfo.m_strOption);
                    }
                    else
                    {
                        LoggerHelper.Error(string.Format("The page to open is null"));
                    }
                }
            }
            else
            {
                LoggerHelper.Error(string.Format("The page to close is null"));
            }

        }

        /// <summary>
        /// 获得页面
        /// </summary>
        /// <param name="pageName"></param>
        /// <returns></returns>
        private UIPage GetPage(string pageName)
        {
            UIPage page = null;
            if(m_dictPageActive.ContainsKey(pageName))
            {
                page = m_dictPageActive[pageName];
            }
            else if(m_dictPageMemory.ContainsKey(pageName))
            {
                page = m_dictPageMemory[pageName];
            }
            else
            {
                GameObject go = UITool.CreateObj(string.Format("Pages/{0}", pageName));
                if(null != go)
                {
                    page = go.GetComponent<UIPage>();
                }
            }

            return page;
        }

        /// <summary>
        /// 打开界面前的处理
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="pageToOpen"></param>
        private void DoBeforeOpen(UIPage currentPage, UIPage pageToOpen)
        {
            if (PageType.FULL_SCREEN == pageToOpen.m_pageType)
            {
                if (currentPage.m_bSaveToHistory)
                {
                    m_stackPageHistory.Push(currentPage.GetPageInfo());
                }
                currentPage.OnForceClose();
            }
        }

        /// <summary>
        /// 关闭页面前的处理
        /// </summary>
        /// <param name="pageToClose"></param>
        /// <param name="pageToOpen"></param>
        private void DoBeforeClose(UIPage pageToClose, UIPage pageToOpen)
        {
        }

        private void RealOpenPage(UIPage pageToOpen, string option)
        {
            m_currentPage = pageToOpen;
            if(pageToOpen.IsOpen())
            {
                pageToOpen.Reopen(option);
            }
            else
            {
                pageToOpen.Open(option);
            }
            
            if(!m_dictPageActive.ContainsKey(pageToOpen.name))
            {
                m_dictPageActive.Add(pageToOpen.name, pageToOpen);
            }
            if(!m_dictPageMemory.ContainsKey(pageToOpen.name))
            {
                m_dictPageMemory.Add(pageToOpen.name, pageToOpen);
            }
        }

        private void RealClosePage(UIPage pageToClose)
        {
            m_dictPageActive.Remove(pageToClose.name);
            pageToClose.Hide();
        }

        private PageInfo HistoryStackTopPage()
        {
            PageInfo topPageInfo = null;
            
            if (null != m_stackPageHistory && 0 != m_stackPageHistory.Count)
            {
                topPageInfo = m_stackPageHistory.Pop();
            }
            return topPageInfo;
        }

    }
}
