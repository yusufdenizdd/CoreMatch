using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T _instance;

    //getter
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("No instance of " + typeof(T) + " exists in the scene.");
                return null;
            }
            else
            {
                return _instance;
            }
        }
    }

    //create reference in Awake()
    protected void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            Init();
        }
        else
        {
            Debug.LogWarning("An instance of " + typeof(T) + " already exists in the scene. Self-destructing.");
            Destroy(gameObject);
        }
    }


    //destroy the reference in OnDestroy()
    protected void OnDestroy()
    {
        if (this == _instance)
        {
            _instance = null;
        }
    }

    // Init will replace the functionality of Awake()
    protected virtual void Init() { }
}
