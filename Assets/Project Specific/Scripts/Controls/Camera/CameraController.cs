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
        _InputManager.OnMouseDragDelta += onMouseDragDelta;
    }
    private void OnDisable()
    {
        if(_InputManager)
            _InputManager.OnMouseDragDelta -= onMouseDragDelta;
    }
    #endregion

    #region Callbacks
    private void onMouseDragDelta(Vector2 dragDelta)
    {
        _CinemachineTransposer.m_Heading.m_Bias += dragDelta.x * _CameraConfiguration.CameraDragSensibility;

        //Vector3 followOffset = _CinemachineTransposer.m_FollowOffset;

        ////float angle = Mathf.Atan2(followOffset.y, followOffset.x);
        //Vector3 camDirectionFromTarget = transform.position - _VirtualCamera.LookAt.position;
        //Vector3 from = camDirectionFromTarget;
        //from.y = 0;
        //from.Normalize();
        //Vector3 to = camDirectionFromTarget;
        //float angle = Vector3.Angle(from, to) * Mathf.Deg2Rad;

        ////angle += dragDelta.y * _CameraConfiguration.CameraDragSensibility * 0.01f;

        //Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized; //this would be the camDirectionFromTarget?
        //Vector3 followOffsetDirection = new Vector3(direction.x, direction.y, direction.x).normalized;

        //Debug.Log($"Camera angle direct: {angle}, Direction: {direction}, FollowOffseetDirection: {followOffsetDirection}");

        //_CinemachineTransposer.m_FollowOffset = followOffsetDirection * followOffset.magnitude;
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

        _CinemachineTransposer.m_FollowOffset = followOffset;
    }
}