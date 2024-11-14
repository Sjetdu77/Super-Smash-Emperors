using UnityEngine;

public class DontDestroyOnLoadManager : MonoBehaviour
{
    public GameObject[] objectsToKeep;

    static public DontDestroyOnLoadManager instance;

    private void Awake()
    {
        if (instance != null) return;
        instance = this;
    }
    void Start()
    {
        foreach (var obj in  objectsToKeep)
        {
            DontDestroyOnLoad(obj);
        }
    }
}
