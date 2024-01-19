using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : SingletonBase<Singleton<T>> where T : MonoBehaviour
{
    private static T s_Instance;
    public bool DontDestroyOnLoad;
    protected bool m_IsDestroyed = false;

    protected virtual void OnAwakeEvent() { }
    public virtual void Start() { }
    public virtual void OnDestroy() { }


    protected sealed override void Awake()
    {
        base.Awake();

        if (s_Instance == null)
        {
            s_Instance = gameObject.GetComponent<T>();
            if (DontDestroyOnLoad) setDontDestroyOnLoad();
            OnAwakeEvent();
        }
        else
        {
            if (this == s_Instance)
            {
                if (DontDestroyOnLoad) setDontDestroyOnLoad();
                OnAwakeEvent();
            }
            else
            {
                m_IsDestroyed = true;
                Destroy(this.gameObject);
            }
        }
    }

    public static T Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = (T)FindObjectOfType(typeof(T));

                if (s_Instance == null)
                {
                    if (applicationIsQuitting && Application.isPlaying)
                    {
                        Debug.LogWarning("[Singleton] IsQuitting Instance '" + typeof(T) + "' is null, returning.");
                        return s_Instance;
                    }
                    else
                    {
                        //create a new gameObject, if Instance isn't found
                        GameObject singleton = new GameObject();
                        s_Instance = singleton.AddComponent<T>();
                        singleton.name = "[Singleton] " + typeof(T).ToString();
                        Debug.Log("[Singleton] An instance of '" + typeof(T) + "' was created: " + singleton);
                    }
                }
            }
            return s_Instance;
        }
    }

    protected static bool applicationIsQuittingFlag = false;
    protected static bool applicationIsQuitting = false;

    private void setDontDestroyOnLoad()
    {
        DontDestroyOnLoad = true;
        if (DontDestroyOnLoad)
        {
            if (transform.parent != null) transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
    }
}
