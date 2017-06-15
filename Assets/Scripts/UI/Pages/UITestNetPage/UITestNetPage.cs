using UnityEngine;
using System.Collections.Generic;
using NetWork;

namespace UI
{
    public class UITestNetPage : UIPage
    {
        public UILabel m_lbServerContent;
        public UILabel m_lbClientContent;

        private NetClient m_client = new NetClient();

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
            m_client.Connent();
        }

        public void OnBtnClientSend()
        {
            m_client.Send(m_lbClientContent.text);
        }

        public void ShowContent(NetModel it)
        {
            if(null != it)
                m_lbServerContent.text = it.Message;
        }
    }
}