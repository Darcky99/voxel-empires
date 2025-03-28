using UnityEngine;

public class CharacterHandler : StateMachinesHandler, IGroundMovement, IVerticalMovement
{
    #region Unity
    protected override void Awake()
    {
        SetStateMachines(
            new GroundMovement_StateMachine(this),
            new VerticalMovement_StateMachine(this)
            );
    }

    #endregion

    [field: SerializeField] public Transform Camera { get; private set; }
    [field: SerializeField] public CharacterController CharacterController { get; private set; }
    [field: SerializeField] public float GroundSpeed { get; private set; }
    [field: SerializeField] public bool Gravity { get; private set; }
}