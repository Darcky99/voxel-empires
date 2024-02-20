using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private InputManager m_InputManager => InputManager.Instance;

    private void OnEnable()
    {
        m_InputManager.OnMove += onMove;
    }
    private void OnDisable()
    {
        m_InputManager.OnMove -= onMove;
    }

    [SerializeField] private CharacterController m_CharacterController;

    private void onMove(Vector2 v)
    {
        v *= 25f * Time.deltaTime;
        //I want to rotate v 45 degrees or 1/2 PI to the right side

        m_CharacterController.Move(new Vector3(v.x, Physics.gravity.y, v.y));
    }
}