using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject
{
    static T s_Instance = null;

    public static T Instance
    {
        get
        {
            if (s_Instance == null)
            {
                T[] i_objects = Resources.FindObjectsOfTypeAll<T>();

                if (i_objects == null || i_objects.Length == 0)
                    i_objects = Resources.LoadAll<T>(string.Empty);

                if (i_objects.Length > 0 && i_objects[0] != null)
                {
                    s_Instance = i_objects[0];
                }

                if (i_objects.Length > 1)
                {
                    Debug.LogError($"More than one {typeof(T).FullName} found, please delete --> { i_objects[1].name}. Using ---> {i_objects[0].name}");
                }
                else if (i_objects.Length == 0)
                {
                    Debug.LogError($"No object {typeof(T).FullName} found, Make sure to implement it.");
                }
            }
            return s_Instance;
        }
    }
}
