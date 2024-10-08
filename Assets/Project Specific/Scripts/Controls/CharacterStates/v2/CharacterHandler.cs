using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VerticalMoment;
using World;
using System;

public class CharacterHandler : StateMachinesHandler, IGroundMovement, IVerticalMovement
{
    #region Unity
    protected override void Awake()
    {
        SetStateMachines(
            new GroundMovement_StateMachine(this),
            new VerticalMovement_StateMachine(this)
            );
    }

    #endregion

    private void onTerrainDrawn()
    {
        //InitializeMachines(); this should be some method like "Allow movement", not initialize the stateMachines
    }

    [field: SerializeField] public Transform Camera { get; private set; }
    [field: SerializeField] public CharacterController CharacterController { get; private set; }
    [field: SerializeField] public float GroundSpeed { get; private set; }
}