using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VerticalMoment;

public class CharacterHandler : StateMachinesHandler, IGroundMovement, IVerticalMovement
{
    protected override void Awake()
    {
        SetStateMachines(
            new GroundMovement_StateMachine(this),
            new VerticalMovement_StateMachine(this)
            );
    }

    [field: SerializeField] public Transform Camera { get; private set; }
    [field: SerializeField] public CharacterController CharacterController { get; private set; }
}