using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateMachine : StateMachineBase<CharacterStateMachine, CharacterStateMachine.eCharacterStates>
{
    private InputManager m_InputManager => InputManager.Instance;

    protected override void Awake()
    {
        Application.targetFrameRate = -1;

        SetMainStates(new CharacterState_NotGrounded(this), new CharacterState_Grounded(this), new CharacterState_Jumping(this));
        SetSubStates(/*new CharacterState_Still(this),*/ new CharacterState_Moving(this));

    }

    public CharacterController CharacterController;
    public bool UseGravity;

    public CinemachineVirtualCamera VirtualCamera;



    public enum eCharacterStates
    {
        Idle, Grounded, NotGrounded, Jumping,
        Still, Moving
    }
}