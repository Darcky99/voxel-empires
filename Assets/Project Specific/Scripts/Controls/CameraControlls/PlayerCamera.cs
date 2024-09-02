using System;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    #region Unity
    private void Start()
    {
        m_CurrentChunkPosition = ChunkUtils.WorldCoordinatesToChunkIndex(transform.position);
    }
    private void Update()
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
            move(direction, vertical);

        if (Input.GetMouseButtonDown(1))
        {
            m_InitialMousePosition = Input.mousePosition;
            m_InitialRotation = transform.eulerAngles;
        }
        else if (Input.GetMouseButton(1))
        {
            rotate((Vector2)Input.mousePosition - m_InitialMousePosition);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            rotate((Vector2)Input.mousePosition - m_InitialMousePosition);
        }
    }
    #endregion

    public static event Action OnCameraMove;

    [SerializeField] private float m_MovementSpeed = 2.5f;
    [SerializeField] private float m_RotationSensibility = 0.15f;

    private Vector3Int m_CurrentChunkPosition;

    private Vector3 m_InitialRotation;
    private Vector2 m_InitialMousePosition;


    private void move(Vector3 direction, int vertical)
    {
        direction.y = 0;
        direction = direction.normalized;
        direction.y = vertical;
        transform.position += direction * m_MovementSpeed * Time.deltaTime;

        Vector3Int currentChunk = ChunkUtils.WorldCoordinatesToChunkIndex(transform.position);

        if (currentChunk != m_CurrentChunkPosition) {
            m_CurrentChunkPosition = currentChunk;
            OnCameraMove?.Invoke();
        }
        
    }
    private void rotate(Vector2 mousePath)
    {
        mousePath *= m_RotationSensibility;

        Vector3 finalRotation = m_InitialRotation;
        finalRotation.y += mousePath.x;
        finalRotation.x -= mousePath.y;
        transform.eulerAngles = finalRotation;
    }
}