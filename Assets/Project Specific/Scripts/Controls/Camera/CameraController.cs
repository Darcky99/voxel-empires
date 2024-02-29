using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class CameraController : MonoBehaviour
{
    private CameraConfiguration m_CameraConfiguration => GameConfig.Instance.CameraConfiguration;
    private InputManager m_InputManager => InputManager.Instance;

    private void Awake()
    {
        m_Distance = m_CameraConfiguration.MinimumDistance;
        m_CinemachineTransposer = m_VirtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
    }
    private void OnEnable()
    {
        m_InputManager.OnMouseScrollDelta += onMouseScrollDelta;
    }
    private void OnDisable()
    {
        m_InputManager.OnMouseScrollDelta -= onMouseScrollDelta;
    }

    private void onMouseScrollDelta(Vector2 scrollDragDelta)
    {
        m_CinemachineTransposer.m_Heading.m_Bias += scrollDragDelta.x * m_CameraConfiguration.CameraDragSensibility;

        Vector3 followOffset = m_CinemachineTransposer.m_FollowOffset;
        float angle = Mathf.Atan2(followOffset.y, followOffset.x);
        angle += scrollDragDelta.y * m_CameraConfiguration.CameraDragSensibility * 0.01f;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
        Vector3 orientation = new Vector3(direction.x, direction.y, direction.x).normalized;
        orientation *= followOffset.magnitude;
        m_CinemachineTransposer.m_FollowOffset = orientation;
    }

    private float m_Distance;

    private CinemachineOrbitalTransposer m_CinemachineTransposer;
    [SerializeField] private CinemachineVirtualCamera m_VirtualCamera;

    private void Update()
    {
        Vector3 followOffset = m_CinemachineTransposer.m_FollowOffset;

        m_Distance = Mathf.Clamp(m_Distance -= (m_InputManager.MouseScrollDelta.y * m_CameraConfiguration.ZoomingSensibility), m_CameraConfiguration.MinimumDistance, m_CameraConfiguration.MaximumDistance);
        followOffset = followOffset.normalized * m_Distance;

        m_CinemachineTransposer.m_FollowOffset = followOffset;

        //m_CinemachineTransposer.m_Heading.m_Bias += ScrollDrag.x;

        //followOffset.x += ScrollDrag.y;
        //followOffset.z += ScrollDrag.y;
        //ScrollDrag
    }
}