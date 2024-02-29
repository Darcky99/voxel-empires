using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VerticalMoment;
using Chunks;
using System;

public class CharacterHandler : StateMachinesHandler, IGroundMovement, IVerticalMovement
{
    protected override void Awake()
    {
        SetStateMachines(
            new GroundMovement_StateMachine(this),
            new VerticalMovement_StateMachine(this)
            );
    }
    protected override void Start() { }

    private void OnEnable()
    {
        ChunkDrawer.OnTerrainDrawn += onTerrainDrawn;
    }
    private void OnDisable()
    {
        ChunkDrawer.OnTerrainDrawn -= onTerrainDrawn;
    }

    private void onTerrainDrawn()
    {
        InitializeMachines();
    }

    [field: SerializeField] public Transform Camera { get; private set; }
    [field: SerializeField] public CharacterController CharacterController { get; private set; }
}