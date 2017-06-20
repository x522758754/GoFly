using UnityEngine;
using System.Collections.Generic;
using NetWork;

namespace UI
{
    public class UITestNetPage : UIPage
    {
        public UILabel m_lbServerContent;
        public UILabel m_lbClientContent;

        private TcpClientWorker m_client = new TcpClientWorker();

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
            m_client.Connect("127.0.0.1", 5555);
        }

        public void OnBtnClientSend()
        {
            //m_client.Send();
        }

        public void ShowContent(NetModel it)
        {
            if(null != it)
                m_lbServerContent.text = it.Message;
        }

        public void OnToggleOne()
        {
            CSLogin msg = new CSLogin();
            msg.deviceKey = "asus";
            msg.ip = "127.0.0.1";
            Packet p = new Packet((int)PBCodeEnum.CSLogin, msg);

            m_client.Send(p);
        }
        public void OnToggleTwo()
        {
            CSHeartBeat msg = new CSHeartBeat();
            msg.clientTime = CommonHelper._UtcNowMs;
            Packet p = new Packet((int)PBCodeEnum.CSHeartBeat, msg);

            m_client.Send(p);
        }
        public void OnToggleThree()
        {

        }

        public void OnDestroy()
        {
            m_client.Release();
        }
        public void OnDisable()
        {
            //m_client.Close();
        }
    }
}