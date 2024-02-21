using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    public event Action<Vector2> OnMove;
    
    private void Update()
    {
        float w = Input.GetKey(KeyCode.W) ?  1 : 0;
        float s = Input.GetKey(KeyCode.S) ? -1 : 0;
        float r = Input.GetKey(KeyCode.D) ?  1 : 0;
        float l = Input.GetKey(KeyCode.A) ? -1 : 0;

        Vector2 v = new Vector2(r + l, w + s);

        if (v.magnitude != 0)
            OnMove?.Invoke(v);
    }
}