using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public abstract class StateBase<StatesMachine, EState> where StatesMachine : StateMachineBase<StatesMachine, EState> where EState : Enum 
{
    /// <summary>
    /// Initialize the State configuration.
    /// </summary>
    /// <param name="stateMachine">The state machine required for this state</param>
    /// <param name="stateKey">Enum value to be used as key for this state</param>
    public StateBase(StatesMachine stateMachine, EState stateKey)
    {
        StateMachine = stateMachine;
        StateKey = stateKey;
    }

    public readonly EState StateKey;

    protected readonly StatesMachine StateMachine;

    #region Physycs
    public virtual void OnCollisionEnter(Collision collision) { }
    public virtual void OnCollisionStay(Collision collision) { }
    public virtual void OnCollisionExit(Collision collision) { }

    public virtual void OnTriggerEnter(Collider other) { }
    public virtual void OnTriggerStay(Collider other) { }
    public virtual void OnTriggerExit(Collider other) { }
    #endregion

    /// <summary>
    /// Use to exit this state.
    /// </summary>
    /// <param name="newState">The new state to transition</param>
    protected void TransitionToState(EState newState)
    {
        StateMachine.TransitionToState(newState);
    }

    protected abstract void OnEnterState();
    protected abstract void OnExitState();
    protected abstract void OnUpdateState();

    /// <summary>
    /// Trigger TransitionToState() in relation to the context.
    /// </summary>
    protected abstract void CheckSwitchState();

    public void EnterState() => OnEnterState();
    public void ExitState() => OnExitState();
    public void UpdateState()
    {
        OnUpdateState();

        CheckSwitchState();
    }
}