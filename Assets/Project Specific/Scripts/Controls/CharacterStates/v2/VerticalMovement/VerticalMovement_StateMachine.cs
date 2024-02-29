using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VerticalMoment
{
    public class VerticalMovement_StateMachine : StateMachineBase<VerticalMovement_StateMachine, eVerticalStates>
    {
        public VerticalMovement_StateMachine(IVerticalMovement iVerticalMovement)
        {
            VerticalMovement = iVerticalMovement;

            SetStates(
                new VerticalMovementState_Grounded(this),
                new VerticalMovementState_Falling(this),
                new VerticalMovementState_Jumping(this)
                );
        }

        public IVerticalMovement VerticalMovement;
    }
}