using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        IPoolableObjectFactory factory = GameObjectPoolableObjectFactory.Instance;

        IObjectPool pool = GameObjectPoolFactory.Instance.CreatePool(factory, 100);

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
