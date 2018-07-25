using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace AssetLoad
{
    public class AssetFileLoader:ResLoader
    {
        public Object Asset
        {
            get { return RetResult as Object; }
        }

        private bool IsLoadAssetBundle;

        public float Progress
        {
            get; set;
        }
    }
}
