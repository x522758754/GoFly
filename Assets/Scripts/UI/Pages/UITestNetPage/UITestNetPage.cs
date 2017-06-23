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
            NetServer.Instance.Send(PBCodeEnum.CSHeartBeat);
        }

        public void OnBtnConnect()
        {
            NetTcpManager.Instance.Init();
            NetTcpManager.Instance.RegisterPushHandler((int)PBCodeEnum.SCHeartBeat, ShowClientContent);
        }

        public void OnBtnClientSend()
        {
            CSLogin msg = new CSLogin();
            msg.deviceKey = "asus";
            msg.ip = "127.0.0.1";

            NetTcpManager.Instance.Send((int)PBCodeEnum.CSLogin, msg, ShowServerContent, "xxx");
        }

        public void ShowServerContent(bool isSuccessm, Packet p, object userObj)
        {
            if (isSuccessm)
                m_lbServerContent.text = string.Format("{0} {1}", p, userObj);
        }

        public void ShowClientContent(Packet p)
        {
            if (null != p)
            {
                m_lbClientContent.text = string.Format("{0}", p);
            }
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