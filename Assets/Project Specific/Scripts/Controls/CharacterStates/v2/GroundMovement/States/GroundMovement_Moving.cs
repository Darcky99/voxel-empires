using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.StateMachine;

public class GroundMovement_Moving : StateBase<GroundMovement_StateMachine, eGroundStates>
{
    private InputManager m_InputManager => InputManager.Instance;

    public GroundMovement_Moving(GroundMovement_StateMachine stateMachine) : base(stateMachine, eGroundStates.Moving) { }

    protected override void OnEnterState() { }
    protected override void OnExitState() { }
    protected override void OnUpdateState()
    {
        Vector2 wasd = m_InputManager.WASD;
        if (wasd.magnitude == 0)
            return;

        float speed = 25f;
        //switch (ParentState.StateKey)
        //{
        //    case CharacterStateMachine.eCharacterStates.Grounded:
        //        speed = 25f;
        //        break;
        //    case CharacterStateMachine.eCharacterStates.NotGrounded:
        //        speed = 18f;
        //        break;
        //    case CharacterStateMachine.eCharacterStates.Jumping:
        //        speed = 27.5f;
        //        break;
        //}
        Vector3 movement = XZMovement(wasd) * speed;
        StateMachine.IGroundMovement.CharacterController.Move(movement);
    }
    protected override void CheckSwitchState()
    {
        if (m_InputManager.WASD == Vector2.zero)
            TransitionToState(eGroundStates.Still);
    }

    private Vector3 XZMovement(Vector2 v)
    {
        Vector3 x = StateMachine.IGroundMovement.Camera.right;
        Vector3 z = StateMachine.IGroundMovement.Camera.forward;
        x.y = 0;
        z.y = 0;
        x = x.normalized * v.x;
        z = z.normalized * v.y;
        Vector3 m = (x + z).normalized * Time.deltaTime;
        return m;
    }
}