using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterState_Still : StateBase<CharacterStateMachine, CharacterStateMachine.eCharacterStates>
{
    private InputManager m_InputManager => InputManager.Instance;

    public CharacterState_Still(CharacterStateMachine stateMachine) : base(stateMachine, CharacterStateMachine.eCharacterStates.Still, null) { }

    protected override void OnEnterState()
    {

    }
    protected override void OnExitState()
    {
    }
    protected override void OnUpdateState() { }

    protected override void CheckSwitchState()
    {
        //if (StateMachine.V != Vector2.zero)
        //    TransitionToState(CharacterStateMachine.eCharacterStates.Moving);
    }
}