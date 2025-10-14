using UnityEngine;
using Utilities.StateMachine;

public class GroundMovementState_Still : StateBase<GroundMovement_StateMachine, eGroundStates>
{
    private InputManager m_InputManager => InputManager.Instance;

    public GroundMovementState_Still(GroundMovement_StateMachine stateMachine) : base(stateMachine, eGroundStates.Still) { }

    protected override void OnEnterState()
    {

    }
    protected override void OnExitState()
    {
    }
    protected override void OnUpdateState() { }

    protected override void CheckSwitchState()
    {
        if (m_InputManager.WASD != Vector2.zero)
            TransitionToState(eGroundStates.Moving);
    }
}