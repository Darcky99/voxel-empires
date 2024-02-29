using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterState_Moving : StateBase<CharacterStateMachine, CharacterStateMachine.eCharacterStates>
{
    private InputManager m_InputManager => InputManager.Instance;

    public CharacterState_Moving(CharacterStateMachine stateMachine) : base(stateMachine, CharacterStateMachine.eCharacterStates.Moving, null) { }

    protected override void OnEnterState() 
    {

    }
    protected override void OnExitState() 
    {

    }
    protected override void OnUpdateState() 
    {
        Vector2 wasd = m_InputManager.WASD;

        if (wasd.magnitude == 0)
            return;

        float speed = 0f;

        switch (ParentState.StateKey)
        {
            case CharacterStateMachine.eCharacterStates.Grounded:
                speed = 25f;
                break;
            case CharacterStateMachine.eCharacterStates.NotGrounded:
                speed = 18f;
                break;
            case CharacterStateMachine.eCharacterStates.Jumping:
                speed = 27.5f;
                break;
        }


        Vector3 movement = XZMovement(wasd) * speed;

        StateMachine.CharacterController.Move(movement);
    }
    protected override void CheckSwitchState()
    {

    }


    private Vector3 XZMovement(Vector2 v)
    {
        Vector3 x = StateMachine.VirtualCamera.transform.right;
        Vector3 z = StateMachine.VirtualCamera.transform.forward;
        x.y = 0;
        z.y = 0;
        x = x.normalized * v.x;
        z = z.normalized * v.y;
        Vector3 m = (x + z).normalized * Time.deltaTime;
        return m;
    }
}