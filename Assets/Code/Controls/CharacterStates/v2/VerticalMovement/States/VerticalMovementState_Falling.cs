using UnityEngine;
using Utilities.StateMachine;

public class VerticalMovementState_Falling : StateBase<VerticalMovement_StateMachine, eVerticalStates>
{
    private CharacterConfiguration m_CharacterConfig => GameConfig.Instance.CharacterConfiguration;
    private float m_Gravity => Physics.gravity.y;

    public VerticalMovementState_Falling(VerticalMovement_StateMachine stateMachine) : base(stateMachine, eVerticalStates.Falling)
    {
        m_FallSpeed = default;
    }

    protected override void OnEnterState()
    {
        m_FallSpeed.y = 0;
        m_TimeFalling = 0f;
    }
    protected override void OnExitState()
    {

    }
    protected override void OnUpdateState()
    {
        StateMachine.VerticalMovement.CharacterController.Move(m_FallSpeed);
        increaseGravity();
    }

    protected override void CheckSwitchState()
    {
        if (StateMachine.VerticalMovement.CharacterController.isGrounded)
            TransitionToState(eVerticalStates.Grounded);
    }

    private float m_TimeFalling;
    private Vector3 m_FallSpeed;

    private void increaseGravity()
    {
        m_TimeFalling += Time.deltaTime;
        m_FallSpeed.y = Mathf.Clamp(m_TimeFalling * m_Gravity * Time.deltaTime, m_Gravity, -m_Gravity);
    }
}