using System;
using UnityEngine;
using VE.VoxelUtilities;

public class PlayerCamera : MonoBehaviour
{
    public static event Action OnCameraMove;

    [SerializeField] private float m_MovementSpeed = 2.5f;
    [SerializeField] private float m_RotationSensibility = 0.15f;

    private Vector2Int _currentChunkPosition;

    private Vector3 _initialRotation;
    private Vector2 _initialMousePosition;

    private void Start()
    {
        _currentChunkPosition = ChunkUtils.WorldCoordinatesToChunkIndex(transform.position);
    }
    private void Update()
    {
        ReadInput();
    }

    private void ReadInput()
    {
        Vector3 direction = Vector3.zero;
        int vertical = 0;

        if (Input.GetKey(KeyCode.W))
            direction += transform.forward;
        if (Input.GetKey(KeyCode.D))
            direction += transform.right;
        if (Input.GetKey(KeyCode.S))
            direction -= transform.forward;
        if (Input.GetKey(KeyCode.A))
            direction -= transform.right;

        if (Input.GetKey(KeyCode.Space))
            vertical = 1;
        if (Input.GetKey(KeyCode.LeftShift))
            vertical = -1;

        if (direction.magnitude != 0 || vertical != 0)
            Move(direction, vertical);

        if (Input.GetMouseButtonDown(1))
        {
            _initialMousePosition = Input.mousePosition;
            _initialRotation = transform.eulerAngles;
        }
        else if (Input.GetMouseButton(1))
        {
            Rotate((Vector2)Input.mousePosition - _initialMousePosition);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            Rotate((Vector2)Input.mousePosition - _initialMousePosition);
        }
    }
    private void Move(Vector3 direction, int vertical)
    {
        direction.y = 0;
        direction = direction.normalized;
        direction.y = vertical;
        transform.position += direction * m_MovementSpeed * Time.deltaTime;
        Vector2Int currentChunk = ChunkUtils.WorldCoordinatesToChunkIndex(transform.position);
        if (currentChunk != _currentChunkPosition)
        {
            _currentChunkPosition = currentChunk;
            OnCameraMove?.Invoke();
        }

    }
    private void Rotate(Vector2 mousePath)
    {
        mousePath *= m_RotationSensibility;
        Vector3 finalRotation = _initialRotation;
        finalRotation.y += mousePath.x;
        finalRotation.x -= mousePath.y;
        transform.eulerAngles = finalRotation;
    }
}