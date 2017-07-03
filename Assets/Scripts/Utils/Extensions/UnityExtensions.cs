/* 原生Unity的扩展方法
 * */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class UnityExtensions
    {

        public static void SetLocalPositionX(this Transform t, float newX)
        {
            t.localPosition = new Vector3(newX, t.localPosition.y, t.localPosition.z);
        }

        public static void SetLocalPositionY(this Transform t, float newY)
        {
            t.localPosition = new Vector3(t.localPosition.x, newY, t.localPosition.z);
        }

        public static void SetLocalPositionZ(this Transform t, float newZ)
        {
            t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y, newZ);
        }

    }
}
