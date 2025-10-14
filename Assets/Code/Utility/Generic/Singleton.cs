using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : SingletonBase<Singleton<T>> where T : MonoBehaviour
{
    private static T s_Instance;

    protected virtual void OnAwakeEvent() { }
    public virtual void Start() { }
    public virtual void OnDestroy() { }


    protected sealed override void Awake()
    {
        base.Awake();

        if (s_Instance == null)
        {
            s_Instance = gameObject.GetComponent<T>();
            OnAwakeEvent();
        }
        else
        {
            if (this == s_Instance)
                OnAwakeEvent();
            else
                Destroy(this.gameObject);
        }
    }

    public static T Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = FindFirstObjectByType<T>(); 

                if (s_Instance == null)
                {
                    if (Application.isPlaying)
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

}