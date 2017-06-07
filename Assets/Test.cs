using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Util;
using System;
using DataLoad;

public class Test:MonoBehaviour
{
    JsonData m_jsonData;
    // Use this for initialization
    void Start ()
    {
        DictManager.Instance.Initialze("Dicts");
        DictManager.Instance.Release();
    }

}
