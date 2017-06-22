using UnityEngine;
using System.Collections.Generic;
using NetWork;
using Util;

namespace UI
{
    public class UITestNetPage : UIPage
    {
        public UILabel m_lbServerContent;
        public UILabel m_lbClientContent;


        protected override void DoOpen()
        {
            base.DoOpen();
        }

        protected override void DoClose()
        {
            base.DoClose();
        }

        public void OnBtnListen()
        {
            NetServer.Instance.Start();
        }

        public void OnBtnServerSend()
        {
        }

        public void OnBtnConnect()
        {
            NetTcpManager.Instance.Init();
        }

        public void OnBtnClientSend()
        {
            CSLogin msg = new CSLogin();
            msg.deviceKey = "asus";
            msg.ip = "127.0.0.1";

            NetTcpManager.Instance.Send((int)PBCodeEnum.CSLogin, msg, ShowContent, "xxx");
        }

        public void ShowContent(bool isSuccessm, Packet p, object userObj)
        {
            if (true)
                m_lbServerContent.text = string.Format("{0} {1}", p, userObj);
        }

        public void OnToggleOne()
        {

        }
        public void OnToggleTwo()
        {
        }
        public void OnToggleThree()
        {

        }

        public void OnDestroy()
        {
            NetTcpManager.Instance.Release();
        }
        public void OnDisable()
        {
            //m_client.Close();
        }

        public void Update()
        {
            NetTcpManager.Instance.Recv();
            TimerHeap.Tick();
        }
    }
}