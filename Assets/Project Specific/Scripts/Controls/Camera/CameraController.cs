using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class CameraController : MonoBehaviour
{
    private CameraConfiguration _CameraConfiguration => GameConfig.Instance.CameraConfiguration;
    private InputManager _InputManager => InputManager.Instance;

    #region Unity
    private void Awake()
    {
        _Distance = _CameraConfiguration.MinimumDistance;
        _CinemachineTransposer = _VirtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
    }
    private void OnEnable()
    {
        _InputManager.OnMouseScrollDelta += onMouseScrollDelta;
    }
    private void OnDisable()
    {
        if(_InputManager)
            _InputManager.OnMouseScrollDelta -= onMouseScrollDelta;
    }
    #endregion

    #region Callbacks
    private void onMouseScrollDelta(Vector2 scrollDragDelta)
    {
        _CinemachineTransposer.m_Heading.m_Bias += scrollDragDelta.x * _CameraConfiguration.CameraDragSensibility;

        Vector3 followOffset = _CinemachineTransposer.m_FollowOffset;
        float angle = Mathf.Atan2(followOffset.y, followOffset.x);
        angle += scrollDragDelta.y * _CameraConfiguration.CameraDragSensibility * 0.01f;
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
        Vector3 orientation = new Vector3(direction.x, direction.y, direction.x).normalized;
        orientation *= followOffset.magnitude;
        _CinemachineTransposer.m_FollowOffset = orientation;
    }
    #endregion

    private float _Distance;

    private CinemachineOrbitalTransposer _CinemachineTransposer;
    [SerializeField] private CinemachineVirtualCamera _VirtualCamera;

    private void Update()
    {
        Vector3 followOffset = _CinemachineTransposer.m_FollowOffset;

        _Distance = Mathf.Clamp(_Distance -= (_InputManager.MouseScrollDelta.y * _CameraConfiguration.ZoomingSensibility), _CameraConfiguration.MinimumDistance, _CameraConfiguration.MaximumDistance);
        followOffset = followOffset.normalized * _Distance;

        //I have to calculate an angle between the camera target and the camera
        //Limit this angle maximum and minimum

        //

        _CinemachineTransposer.m_FollowOffset = followOffset;
    }
}