using UnityEngine;
//using System.Collections;
using System.Collections.Generic;
//using Com.Hcwy.H1.DictEnum;

namespace UI
{
//     public enum PageType
//     {
//         FULL_SCREEN,
//         COVER,
//     }

    //页面管理器
    public class PageManager : MonoBehaviour
    {
        public List<Page> memoryPages;

        private Dictionary<string, Page> pageDictonary;

        public List<string> pageHostory = new List<string>(); //操作栈

        public List<string> alwaysInMemoryAtlas;

        // 当前页面 
        public Page currentPage;

        public HashSet<Page> activePage = new HashSet<Page>();


        void Awake()
        {
            instance = this;
            InitPages();
        }

        public void OpenHostory(string hostory)
        {
            char[] sc = { '?' };
            string[] sep = hostory.Split(sc);
            Page pageToOpen = GetPage(sep[0]);
            currentPage = pageToOpen;
            SimpleOpenPage(pageToOpen, sep[1]);

            while (pageToOpen.pageType == PageType.COVER)
            {
                if (pageHostory.Count > 0)
                {
                    pageHostory.RemoveAt(pageHostory.Count - 1);
                    string[] sep2 = hostory.Split(sc);
                    pageToOpen = GetPage(sep2[0]);
                    SimpleOpenPage(pageToOpen, sep2[1]);
                }
                break;
            }
        }

        private void InitPages()
        {
            if (pageDictonary == null)
            {
                pageDictonary = new Dictionary<string, Page>();
            }
        }


        public void DestoryMemoryPage()
        {
            foreach (Page page in memoryPages)
            {
                if (page == null)
                {
                    Debug.LogError("memory page be destory in error mode");
                }
                else
                {
                    page.OnMemoryPageDestory();
                }
            }

        }

        public void OpenPage(string pageName, string option)
        {
            Debug.Log("Open page " + pageName + " option " + option);
            if (currentPage != null && currentPage.name == pageName)
            {
                currentPage.Reopen(option);
                return;
            }
            Page pageToOp = GetPage(pageName);
            if (pageToOp == null)
            {
                Debug.LogError("Cannot find the page " + pageName);
                return;
            }

            if (currentPage != null)
            {
                if (pageToOp.pageType == PageType.COVER)
                {
                    if (currentPage.pageType == PageType.FULL_SCREEN)
                    {
                        currentPage.OnCoverPageOpen();
                        string pao = currentPage.name + "?" + ((currentPage.saveToHistory) ? 1 : 0) + "?" + currentPage.GetOptString();
                        pageHostory.Add(pao);
                    }
                    else
                    {
                        currentPage.CorverdForceClose();
                        SimpleClosePage(currentPage);
                    }
                }
                else
                {
                    Page pageToClose;
                    if (currentPage.pageType == PageType.COVER)
                    {
                        currentPage.CorverdForceClose();
                        SimpleClosePage(currentPage);
                        string pn, options;
                        GetClosestPage(false, out pn, out options);
                        pageToClose = GetPage(pn);
                    }
                    else
                    {
                        pageToClose = currentPage;
                    }
                    if (pageToClose.saveToHistory)
                    {
                        string pao = pageToClose.name + "?" + 1 + "?" + pageToClose.GetOptString();
                        pageHostory.Add(pao);
                    }

                    pageToClose.OnForceClose();
                    SimpleClosePage(pageToClose);

                }
                //if (currentPage.pageType == PageType.FULL_SCREEN)
                //{
                //    string pao = currentPage.name + "?" + ((currentPage.saveToHistory) ? 1 : 0) + "?" + currentPage.GetOptString();
                //    pageHostory.Add(pao);
                //    if (pageToOp.pageType == PageType.FULL_SCREEN)
                //    {
                //        currentPage.OnForceClose();
                //        SimpleClosePage(currentPage);
                //        //for (int i = pageHostory.Count - 1; i >= 0; --i)
                //        //{
                //        //    string pn = pageHostory[i];
                //        //    int indexQ = pn.IndexOf('?');
                //        //    if (indexQ != -1)
                //        //    {
                //        //        pn = pn.Substring(0, indexQ);
                //        //    }
                //        //    if (pageDictonary.ContainsKey(pn))
                //        //    {
                //        //        var pageToClose = pageDictonary[pn];
                //        //        if (pageToClose.isOpen)
                //        //        {
                //        //            pageDictonary[pn].OnForceClose();
                //        //            SimpleClosePage(pageDictonary[pn]);
                //        //        }
                //        //        if (pageToClose.pageType == PageType.FULL_SCREEN)
                //        //        {
                //        //            break;
                //        //        }
                //        //    }
                //        //    //else
                //        //    //{
                //        //    //    break;
                //        //    //}
                //        //}
                //    }
                //    else
                //    {
                //        currentPage.OnCoverPageOpen();
                //    }
                //}
                //else
                //{
                //    SimpleClosePage(currentPage);
                //}
            }

            currentPage = pageToOp;
            SimpleOpenPage(pageToOp, option);
        }

        public void OnClosePage(Page pageForClose)
        {
            if (pageForClose != currentPage)
            {
                Debug.LogError("page close error! closed page " + pageForClose.name + " is not the current page " + currentPage.name);
                SimpleClosePage(pageForClose);
                return;
            }
            bool isCover = currentPage.pageType == PageType.COVER;
            Page pageToClose = currentPage;
            string prePageName;
            string prePageOption;
            //string[] sep;
            //char[] sc = { '?' };
            //if (pageHostory.Count == 0)
            //{
            //    //sep = new string[2];
            //    prePageName = "TownPage";
            //    prePageOption = "";
            //}
            //else
            //{
            while (true)
            {
                if (pageHostory.Count == 0)
                {
                    //sep = new string[2];
                    prePageName = "TownPage";
                    prePageOption = "";
                    break;
                }
                string pao = pageHostory[pageHostory.Count - 1];
                pageHostory.RemoveAt(pageHostory.Count - 1);
                if (pageHostory.Count > 1 && pageHostory[pageHostory.Count - 1].StartsWith(currentPage.name + "?"))
                {
                    pageHostory.RemoveAt(pageHostory.Count - 1);
                    continue;
                }
                bool isHostory = AnalysHistory(pao, out prePageName, out prePageOption);
                if (isCover || (isHostory && prePageName != pageForClose.name))
                {
                    break;
                }
                //if ()
                //{
                //    if(prePageName == pageForClose.name)
                //    {
                //        continue;
                //    }
                //    else
                //    {
                //        break;
                //    }
                //}
                //else
                //{
                //    if(isCover)
                //    {
                //        break;
                //    }
                //    //else if(pageDictonary.ContainsKey(prePageName) && pageDictonary[prePageName].isOpen)
                //    //{
                //    //    SimpleClosePage(pageDictonary[prePageName]);
                //    //}
                //}
            }
            Page prePage = GetPage(prePageName);
            currentPage = prePage;
            if (isCover)
            {
                SimpleClosePage(pageToClose);
                RefreshCommon(currentPage);
                currentPage.OnCoverPageRemove();
                currentPage.Show();
                return;
            }

            if (prePage != null)
            {
                SimpleOpenPage(prePage, prePageOption);

                //if (prePage.pageType != PageType.FULL_SCREEN)
                //{
                //    for (int i = pageHostory.Count - 1; i >= 0; --i)
                //    {
                //        string pageName, pageOption;
                //        //string[] sep2 = pageHostory[i].Split(sc);
                //        if (!AnalysHistory(pageHostory[i], out pageName, out pageOption))
                //        {
                //            continue;
                //        }
                //        Page popPage = GetPage(pageName);
                //        SimpleOpenPage(popPage, pageOption);
                //        if (popPage.pageType == PageType.FULL_SCREEN)
                //        {
                //            break;
                //        }
                //    }
                //}
            }
            else
            {
                Debug.LogError("page dictionary error");
            }
            SimpleClosePage(pageToClose);
        }

        public void GetClosestPage(bool isFull, out string pageName, out string options)
        {
            if (pageHostory.Count == 0)
            {
                //sep = new string[2];
                pageName = "TownPage";
                options = "";
                return;
            }
            if (!isFull)
            {
                //上一个页面一定是个全屏页面
                string pao = pageHostory[pageHostory.Count - 1];
                pageHostory.RemoveAt(pageHostory.Count - 1);
                AnalysHistory(pao, out pageName, out options);
                if (pageDictonary.ContainsKey(pageName))
                {
                    if (pageDictonary[pageName].pageType == PageType.FULL_SCREEN)
                    {
                        //正常逻辑是这样的
                        return;
                    }
                    else
                    {
                        Debug.LogError("Cover page " + pageName + " be opened on cover page");
                    }
                }
                else
                {
                    Debug.LogError("pre page " + pageName + " not in dictionary");
                }
            }
            else
            {
                while (true)
                {
                    string pao = pageHostory[pageHostory.Count - 1];
                    pageHostory.RemoveAt(pageHostory.Count - 1);
                    //bool isHistory = ;
                    if (AnalysHistory(pao, out pageName, out options))
                    {
                        break;
                    }
                }
            }
        }

        public Page GetDictPage(string pageName)
        {
            if (pageDictonary.ContainsKey(pageName))
            {
                return pageDictonary[pageName];
            }
            else
            {
                return null;
            }
        }

        private Page GetPage(string pageName)
        {
            Page page = null;
            if (pageDictonary == null)
            {
                pageDictonary = new Dictionary<string, Page>();
            }
            if (pageDictonary.ContainsKey(pageName))
            {
                page = pageDictonary[pageName];
            }
            else
            {
                Object obj = Resources.Load("Pages/" + pageName, typeof(GameObject));
                if (obj != null)
                {
                    GameObject gObj = Instantiate(obj) as GameObject;
                    gObj.transform.parent = transform;
                    gObj.transform.localScale = Vector3.one;
                    gObj.name = pageName;
                    page = gObj.GetComponent<Page>();

                    if (page != null)
                    {
                        pageDictonary[pageName] = page;
                        page.pageManager = this;
                        if (page.alwaysInMemery)
                        {
                            memoryPages.Add(page);
                        }
                    }
                }
            }
            return page;
        }

        private void SimpleOpenPage(Page page, string option)
        {
            RefreshCommon(currentPage);
            if (page.pageType == PageType.FULL_SCREEN)
            {
                //用回调来做吧
                //ManagerController.Instance.cameraTool.sceneCamera.enabled = page.showScene;
            }
            //FreshTipAndBarState(page);
            if (page.isOpen)
            {
                page.Reopen(option);
            }
            else
            {
                page.Open(option);
            }
            if (!activePage.Contains(page))
            {
                activePage.Add(page);
            }
        }

        private void SimpleClosePage(Page page)
        {
            activePage.Remove(page);
            page.Hide();
            foreach (Texture2D ua in page.atlasOfPage)
            {
                bool cannotUnload = false;
                foreach (Page p in activePage)
                {
                    foreach (Texture2D uia in p.atlasOfPage)
                    {
                        if (uia == ua)
                        {
                            cannotUnload = true;
                            break;
                        }
                    }
                    if (cannotUnload)
                    {
                        break;
                    }
                }
                if (ua != null)
                {
                    if (!cannotUnload && alwaysInMemoryAtlas.IndexOf(ua.name) == -1)
                    {
                        Resources.UnloadAsset(ua);
                    }
                }
            }
            if (!page.alwaysInMemery)
            {
                pageDictonary.Remove(page.name);
                Destroy(page.gameObject);
            }
            //AssetLoad.ResourceManager.Instance.UnloadUnusedResources();
        }




        public static PageManager instance;
        public static PageManager Instance
        {
            get
            {
                return instance;
            }
        }
        public void OnDestory()
        {
            instance = null;
        }

        public void HideObject(GameObject obj)
        {
            obj.transform.localPosition = new Vector3();
        }

        public void UnLoadUIAtlasAssets()
        {
            if (currentPage == null)
                return;
            foreach (Texture2D ua in currentPage.atlasOfPage)
            {
                if (ua != null)
                {
                    Resources.UnloadAsset(ua);
                }
            }
            if (pageDictonary == null)
                return;
            foreach (Page page in pageDictonary.Values)
            {
                foreach (Texture2D ua in page.atlasOfPage)
                {
                    if (ua != null)
                    {
                        Resources.UnloadAsset(ua);
                    }
                }
            }
        }

        public void ChangeToBattle()
        {
            //EasyTouchTool.Instance.Hide();
            HideUI();
            Resources.UnloadUnusedAssets();
        }
        public void Clear()
        {
            memoryPages.Clear();
            activePage.Clear();
            pageHostory.Clear();
            foreach (var p in pageDictonary)
            {
                if (p.Value != null)
                {
                    Destroy(p.Value.gameObject);
                    Destroy(p.Value);
                }
            }
            pageDictonary.Clear();
            currentPage = null;
            //HideUI();
        }

        public void ChangeUIBack()
        {
            ShowUI();
            //使用回调处理一些特殊情况
        }

        public void HideUI()
        {
            //transform.localPosition = new Vector3(10000f, 10000f, 10000f);
            gameObject.SetActive(false);
        }
        public void ShowUI()
        {
            //transform.localPosition = Vector3.zero;
            gameObject.SetActive(true);
        }


        public void OnDestroy()
        {
            instance = null;
            if (pageDictonary != null)
            {
                pageDictonary.Clear();
            }
        }

        public T GetPage<T>() where T : Page
        {
            T retPage = null;
            foreach (Page page in pageDictonary.Values)
            {
                retPage = page.gameObject.GetComponent<T>();
                if (retPage != null)
                {
                    return retPage;
                }
            }
            return retPage;

        }


        #region 辅助显示

        //public TopBar topBar;
        //public void FreshTipAndBarState(Page page)
        //{
        //    if (topBar == null)
        //        return;
        //    if (page.showTopBar)
        //    {
        //        topBar.Show();
        //    }
        //    else
        //    {
        //        topBar.Hide();
        //    }
        //}
        public void RefreshCommon(Page page)
        {
            if (page.showEasyTouch)
            {
                //EasyTouchTool.Instance.ShowJoystick();
            }
            else
            {
                //EasyTouchTool.Instance.Hide();
            }
            //if (page.pageType == PageType.FULL_SCREEN)
            //{
            //    ManagerController.Instance.cameraTool.sceneCamera.enabled = page.showScene;
            //}
        }

        private bool AnalysHistory(string history, out string pageName, out string option)
        {
            char[] sc = { '?' };
            string[] hs = history.Split(sc);
            if (hs.Length != 3)
            {
                Debug.LogError("history error " + history);
            }
            pageName = hs[0];
            option = hs[2];
            return hs[1] == "1";
        }

        #endregion
    }
}
