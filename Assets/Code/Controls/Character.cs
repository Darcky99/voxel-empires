using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Character : MonoBehaviour
{
    private InputManager m_InputManager => InputManager.Instance;

    #region Init
    private void Awake()
    {
        UseGravity = false;

        m_Movement = new Vector3();
    }
    private void OnEnable()
    {
        //m_InputManager.OnMove += onMove;
        //m_InputManager.OnSpacePress += onSpacePress;
    }
    private void OnDisable()
    {
        if (m_InputManager == null)
            return;

        //m_InputManager.OnMove -= onMove;
        //m_InputManager.OnSpacePress -= onSpacePress;
    }
    #endregion

    #region Loop
    
    private void LateUpdate()
    {
        move();
    }
    #endregion



    #region Callbacks
    private void onMove(Vector2 v)
    {
        Vector3 x = m_VirtualCamera.transform.right;
        Vector3 z = m_VirtualCamera.transform.forward;
        x.y = 0;
        z.y = 0;
        x.Normalize();
        z.Normalize();
        x *= v.x;
        z *= v.y;
        float sample_y = m_Movement.y;
        m_Movement = (x + z).normalized;
        m_Movement *= 25f * Time.deltaTime;
        m_Movement.y = sample_y;
    }
    private void onSpacePress()
    {
        //disable gravity falling for a moment, then apply jumping
    }
    #endregion

    [SerializeField] private CharacterController m_CharacterController;
    [SerializeField] public bool UseGravity;

    [SerializeField] private CinemachineVirtualCamera m_VirtualCamera;

    private Vector3 m_Movement;

    private void move()
    {
        m_CharacterController.Move(m_Movement);
        //m_Movement.x = 0;
        //m_Movement.z = 0;
    }
}