using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    public Vector2 WASD { get; private set; }
    public bool Space { get; private set; }

    public Vector2 MouseScrollDelta { get; private set; }

    public event Action<Vector2> OnMouseScrollDelta;

    public Vector2 ScrollDragDelta;

    private Vector2 m_LastMousePosition;

    private void Update()
    {
        updateWASD();
        updateSpace();
        updateMouseScrollDelta();
        MouseScrollDelta = Input.mouseScrollDelta;
        m_LastMousePosition = Input.mousePosition;
    }
    private void updateWASD()
    {
        float w = Input.GetKeyDown(KeyCode.W) || Input.GetKey(KeyCode.W) ? 1 : 0;
        float s = Input.GetKeyDown(KeyCode.S) || Input.GetKey(KeyCode.S) ? -1 : 0;
        float r = Input.GetKeyDown(KeyCode.D) || Input.GetKey(KeyCode.D) ? 1 : 0;
        float l = Input.GetKeyDown(KeyCode.A) || Input.GetKey(KeyCode.A) ? -1 : 0;
        Vector2 v = new Vector2(r + l, w + s);
        WASD = v;
    }
    private void updateSpace()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.Space))
            Space = true;
        else
            Space = false;
    }
    private void updateMouseScrollDelta()
    {
        if (Input.GetMouseButtonDown(2) || Input.GetMouseButton(2))
        {
            Vector2 scrollDragDelta =  (Vector2)Input.mousePosition - m_LastMousePosition;
            OnMouseScrollDelta?.Invoke(scrollDragDelta);
            Debug.Log(scrollDragDelta);
        }
    }
}