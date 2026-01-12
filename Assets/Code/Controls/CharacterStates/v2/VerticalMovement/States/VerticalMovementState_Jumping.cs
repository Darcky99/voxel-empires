using Utilities.StateMachine;
using UnityEngine;
using VoxelEmpires.Configuration;

namespace VoxelEmpires.Player
{
    public class VerticalMovementState_Jumping : StateBase<VerticalMovement_StateMachine, eVerticalStates>
    {
        private CharacterConfiguration m_CharacterConfig => GameConfig.Instance.CharacterConfiguration;
        private InputManager m_InputManager => InputManager.Instance;

        public VerticalMovementState_Jumping(VerticalMovement_StateMachine stateMachine) : base(stateMachine, eVerticalStates.Jumping) { }

        protected override void OnEnterState()
        {
            m_TimeJumping = 0f;
            ProgressJump();
        }
        protected override void OnExitState() { }
        protected override void OnUpdateState()
        {
            if (m_InputManager.Space && m_TimeJumping < m_CharacterConfig.JumpDuration)
            {
                ProgressJump();
            }
        }
        protected override void CheckSwitchState()
        {
            if (!m_InputManager.Space || m_TimeJumping >= m_CharacterConfig.JumpDuration)
            {
                TransitionToState(eVerticalStates.Falling);
            }
        }

        private float m_TimeJumping;

        private void ProgressJump()
        {
            float aTime = m_TimeJumping / m_CharacterConfig.JumpDuration;
            float previousHeight = m_CharacterConfig.JumpCurve.Evaluate(aTime) * m_CharacterConfig.JumpHeight;
            m_TimeJumping += Time.deltaTime;
            float bTime = m_TimeJumping / m_CharacterConfig.JumpDuration;
            float nextHeight = m_CharacterConfig.JumpCurve.Evaluate(bTime) * m_CharacterConfig.JumpHeight;
            Vector3 jump = new Vector3(0, nextHeight - previousHeight, 0);
            StateMachine.VerticalMovement.CharacterController.Move(jump);
        }
    }
}