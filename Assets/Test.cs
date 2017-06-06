using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Util;
using System;

public class Test:MonoBehaviour
{
    JsonData m_jsonData;
    // Use this for initialization
    void Start ()
    {
        string str = string.Empty;
        str = DateTime.UtcNow.Ticks.ToString();
        str = DateTime.Now.Ticks.ToString(); ;

        long Ticks = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        long Ticks1 = (DateTime.UtcNow.Ticks - 621355968000000000) / 10000000;

        DateTime d = new DateTime(Ticks);
        DateTime d1 = new DateTime(Ticks, DateTimeKind.Local);
        DateTime d2 = new DateTime(Ticks, DateTimeKind.Unspecified);
        DateTime d3 = new DateTime(Ticks, DateTimeKind.Utc);
    }

}
