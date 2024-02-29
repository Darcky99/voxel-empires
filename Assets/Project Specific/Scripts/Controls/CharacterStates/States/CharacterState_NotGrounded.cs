using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterState_NotGrounded : StateBase<CharacterStateMachine, CharacterStateMachine.eCharacterStates>
{
    public CharacterState_NotGrounded(CharacterStateMachine stateMachine) : base(stateMachine, CharacterStateMachine.eCharacterStates.NotGrounded, new[] {
        //CharacterStateMachine.eCharacterStates.Still,
        CharacterStateMachine.eCharacterStates.Moving
    })
    {
        m_Gravity = default;
    }

    protected override void OnEnterState()
    {
        //increase gravity +- in the same time you increase the jump.
    }
    protected override void OnExitState()
    {

    }
    protected override void OnUpdateState() 
    {
        StateMachine.CharacterController.Move(Physics.gravity * 8 * Time.deltaTime);
    }

    protected override void CheckSwitchState()
    {
        if (StateMachine.CharacterController.isGrounded)
            TransitionToState(CharacterStateMachine.eCharacterStates.Grounded);
    }


    private float m_TimeFalling;
    private Vector3 m_Gravity;

    private void increaseGravity()
    {
        float aTime = m_TimeJumping / m_CharacterConfig.JumpDuration;
        float previousHeight = m_CharacterConfig.JumpCurve.Evaluate(aTime) * m_CharacterConfig.JumpHeight;
        m_TimeJumping += Time.deltaTime;
        float bTime = m_TimeJumping / m_CharacterConfig.JumpDuration;
        float nextHeight = m_CharacterConfig.JumpCurve.Evaluate(bTime) * m_CharacterConfig.JumpHeight;
        Vector3 jump = new Vector3(0, nextHeight - previousHeight, 0);
        StateMachine.CharacterController.Move(jump);

        totalHeight += nextHeight - previousHeight;
    }
}