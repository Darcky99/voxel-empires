using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    //public event Action<Vector2> OnMove;
    //public event Action OnSpacePress;

    public Vector2 WASD { get; private set; }
    public bool Space { get; private set; }
    
    private void Update()
    {
        float w = Input.GetKeyDown(KeyCode.W) || Input.GetKey(KeyCode.W) ?  1 : 0;
        float s = Input.GetKeyDown(KeyCode.S) || Input.GetKey(KeyCode.S) ? -1 : 0;
        float r = Input.GetKeyDown(KeyCode.D) || Input.GetKey(KeyCode.D) ?  1 : 0;
        float l = Input.GetKeyDown(KeyCode.A) || Input.GetKey(KeyCode.A) ? -1 : 0;
        Vector2 v = new Vector2(r + l, w + s);
        WASD = v;




        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.Space))
            Space = true;
        else
            Space = false;
    }
}