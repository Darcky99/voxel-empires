using UnityEngine;
using Cinemachine;
using System;
using VoxelEmpires.Configuration;

public class CameraController : Singleton<CameraController>
{
    private CameraConfiguration _CameraConfiguration => GameConfig.Instance.CameraConfiguration;
    private InputManager _InputManager => InputManager.Instance;

    public event EventHandler Move;

    #region Unity
    protected override void OnAwakeEvent()
    {
        base.OnAwakeEvent();
        _Distance = _CameraConfiguration.MinimumDistance;
        _CinemachineTransposer = _VirtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        RegisterCurrentCameraPosition();
    }
    private void OnEnable()
    {
        _InputManager.OnMouseDragDelta += OnMouseDragDelta;
    }
    private void OnDisable()
    {
        if (_InputManager)
        {
            _InputManager.OnMouseDragDelta -= OnMouseDragDelta;
        }
    }
    #endregion

    #region Callbacks
    private void OnMouseDragDelta(Vector2 dragDelta)
    {
        _CinemachineTransposer.m_Heading.m_Bias += dragDelta.x * _CameraConfiguration.CameraDragSensibility;
    }
    #endregion

    private float _Distance;
    private Vector3 _LastPosition;
    private Quaternion _LastRotation;

    private CinemachineOrbitalTransposer _CinemachineTransposer;
    [SerializeField] private CinemachineVirtualCamera _VirtualCamera;

    private void Update()
    {
        CalculateFollowOffset();
        DetectMovent();
    }

    private void CalculateFollowOffset()
    {
        Vector3 followOffset = _CinemachineTransposer.m_FollowOffset;
        _Distance = Mathf.Clamp(_Distance -= (_InputManager.MouseScrollDelta.y * _CameraConfiguration.ZoomingSensibility), _CameraConfiguration.MinimumDistance, _CameraConfiguration.MaximumDistance);
        // _Distance = Mathf.Clamp(_Distance -= (_InputManager.MouseScrollDelta.y * 1.678f), 42, 168);
        followOffset = followOffset.normalized * _Distance;
        _CinemachineTransposer.m_FollowOffset = followOffset;
    }

    private void RegisterCurrentCameraPosition()
    {
        _LastPosition = transform.position;
        _LastRotation = transform.rotation;
    }
    private void DetectMovent()
    {
        if (_LastPosition != transform.position || _LastRotation != transform.rotation)
        {
            Move?.Invoke(this, EventArgs.Empty);
            RegisterCurrentCameraPosition();
        }
    }
}