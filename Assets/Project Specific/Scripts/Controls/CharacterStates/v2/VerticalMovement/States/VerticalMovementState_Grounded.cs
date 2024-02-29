using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VerticalMoment
{
    public class VerticalMovementState_Grounded : StateBase<VerticalMovement_StateMachine, eVerticalStates>
    {
        private InputManager m_InputManager => InputManager.Instance;

        public VerticalMovementState_Grounded(VerticalMovement_StateMachine stateMachine) : base(stateMachine, eVerticalStates.Grounded) { }

        protected override void OnEnterState()
        {

        }
        protected override void OnExitState()
        {

        }
        protected override void OnUpdateState()
        {
            StateMachine.VerticalMovement.CharacterController.Move(Physics.gravity * 0.1f  * Time.deltaTime);
        }
        protected override void CheckSwitchState()
        {
            if (StateMachine.VerticalMovement.CharacterController.isGrounded == false)
                TransitionToState(eVerticalStates.Falling);
            else if (m_InputManager.Space)
                TransitionToState(eVerticalStates.Jumping);
        }
    }
}