using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterState_Idle : StateBase<CharacterStateMachine, CharacterStateMachine.eCharacterStates>
{
    private InputManager m_InputManager => InputManager.Instance;

    public CharacterState_Idle(CharacterStateMachine stateMachine) : base(stateMachine, CharacterStateMachine.eCharacterStates.Idle, new[] {
        //CharacterStateMachine.eCharacterStates.Still,
        CharacterStateMachine.eCharacterStates.Moving
    })
    {
        m_Gravity = default;
    }

    protected override void OnEnterState()
    {

    }
    protected override void OnExitState()
    {

    }
    protected override void OnUpdateState()
    {
        m_Gravity.y = -40f * Time.deltaTime;
        StateMachine.CharacterController.Move(m_Gravity);
    }

    protected override void CheckSwitchState()
    {
        if (StateMachine.CharacterController.isGrounded)
            TransitionToState(CharacterStateMachine.eCharacterStates.Grounded);

        else if (StateMachine.CharacterController.isGrounded && m_InputManager.Space)
            TransitionToState(CharacterStateMachine.eCharacterStates.Jumping);
    }

    //movement + gravity

    private Vector3 m_Gravity;
}