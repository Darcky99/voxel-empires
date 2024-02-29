using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VerticalMoment
{
    public class VerticalMovementState_Jumping : StateBase<VerticalMovement_StateMachine, eVerticalStates>
    {
        private CharacterConfiguration m_CharacterConfig => GameConfig.Instance.CharacterConfiguration;
        private InputManager m_InputManager => InputManager.Instance;

        public VerticalMovementState_Jumping(VerticalMovement_StateMachine stateMachine) : base(stateMachine, eVerticalStates.Jumping) { }

        protected override void OnEnterState()
        {
            m_TotalHeight = 0;
            m_TimeJumping = 0f;
            progressJump();
        }
        protected override void OnExitState()
        {

        }
        protected override void OnUpdateState()
        {
            if (m_InputManager.Space && m_TimeJumping < m_CharacterConfig.JumpDuration)
                progressJump();
        }
        protected override void CheckSwitchState()
        {
            if (!m_InputManager.Space || m_TimeJumping >= m_CharacterConfig.JumpDuration)
                TransitionToState(eVerticalStates.Falling);
        }

        private float m_TotalHeight;
        private float m_TimeJumping;

        private void progressJump()
        {
            float aTime = m_TimeJumping / m_CharacterConfig.JumpDuration;
            float previousHeight = m_CharacterConfig.JumpCurve.Evaluate(aTime) * m_CharacterConfig.JumpHeight;
            m_TimeJumping += Time.deltaTime;
            float bTime = m_TimeJumping / m_CharacterConfig.JumpDuration;
            float nextHeight = m_CharacterConfig.JumpCurve.Evaluate(bTime) * m_CharacterConfig.JumpHeight;
            Vector3 jump = new Vector3(0, nextHeight - previousHeight, 0);
            StateMachine.VerticalMovement.CharacterController.Move(jump);

            m_TotalHeight += nextHeight - previousHeight;
        }
    }
}