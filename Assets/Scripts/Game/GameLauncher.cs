using UnityEngine;
using DataLoad;

public class GameLauncher : MonoBehaviour
{
    private void Awake()
    {
        GameObject obj = new GameObject("UIRoot");
        DontDestroyOnLoad(obj);
    }

    private void Start()
    {
        DictManager.Instance.Initialze("Dicts");
    }
}
