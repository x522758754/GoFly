using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        RunCoroutine.Instance.Run(ss());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator ss()
    {
        print(1);
        yield return 0;
        print(2);

        yield return 0;
        print(3);
    }
}
