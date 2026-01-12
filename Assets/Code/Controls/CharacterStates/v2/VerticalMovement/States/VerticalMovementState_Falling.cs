using UnityEngine;
using Utilities.StateMachine;
using VoxelEmpires.Configuration;

namespace VoxelEmpires.Player
{
    public class VerticalMovementState_Falling : StateBase<VerticalMovement_StateMachine, eVerticalStates>
    {
        private CharacterConfiguration CharacterConfig
        {
            get => GameConfig.Instance.CharacterConfiguration;
        }
        private float Gravity
        {
            get => Physics.gravity.y;
        }

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
            m_FallSpeed.y = Mathf.Clamp(m_TimeFalling * Gravity * Time.deltaTime, Gravity, -Gravity);
        }
    }
}