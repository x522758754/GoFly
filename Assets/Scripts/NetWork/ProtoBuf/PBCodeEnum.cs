using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetWork
{
    public enum PBCodeEnum
    {
        #region 错误信息 10000
        ErrorMessage = 10001,
        #endregion
        #region 基本通讯 20000
        CSLogin = 20001,
        SCLogin = 20002,
        CSHeartBeat = 20003,
        SCHeartBeat = 20004,
        #endregion
    }

}
