using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterState_Grounded : StateBase<CharacterStateMachine, CharacterStateMachine.eCharacterStates>
{
    private InputManager m_InputManager => InputManager.Instance;

    public CharacterState_Grounded(CharacterStateMachine stateMachine) : base(stateMachine, CharacterStateMachine.eCharacterStates.Grounded, new[] {
        //CharacterStateMachine.eCharacterStates.Still,
        CharacterStateMachine.eCharacterStates.Moving
    }) { }

    protected override void OnEnterState()
    {

    }
    protected override void OnExitState() 
    {

    }
    protected override void OnUpdateState() 
    {
        //m_Gravity.y = -;
        StateMachine.CharacterController.Move(Physics.gravity * 8 * Time.deltaTime);
    }
    protected override void CheckSwitchState() 
    {
        if (StateMachine.CharacterController.isGrounded == false)
            TransitionToState(CharacterStateMachine.eCharacterStates.NotGrounded);
        else if (m_InputManager.Space && StateMachine.CharacterController.isGrounded)
            TransitionToState(CharacterStateMachine.eCharacterStates.Jumping);
    }

    private Vector3 m_Gravity;
}