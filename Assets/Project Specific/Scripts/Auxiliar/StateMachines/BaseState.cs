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
    /// <param name="subStates">SubStates availible for this state. Leave as null if this is not meant to have subStates</param>
    public StateBase(StatesMachine stateMachine, EState stateKey, EState[] subStates = null)
    {
        StateMachine = stateMachine;
        StateKey = stateKey;

        if(subStates != null)
            SubStates = subStates.ToList();
    }

    public readonly EState StateKey;

    public StateBase<StatesMachine, EState> ParentState { get; protected set; }
    public StateBase<StatesMachine, EState> CurrentSubState { get; protected set; }

    protected readonly StatesMachine StateMachine;
    protected readonly List<EState> SubStates;

    private bool m_IsTransitionSubStateState;


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
        if (ParentState != null)
            ParentState.TransitionToSubState(newState);
        else
            StateMachine.TransitionToState(newState);
    }

    protected abstract void OnEnterState();
    protected abstract void OnExitState();
    protected abstract void OnUpdateState();

    /// <summary>
    /// Trigger TransitionToState() in relation to the context.
    /// </summary>
    protected abstract void CheckSwitchState();

    public void EnterState()
    {
        OnEnterState();

        if(SubStates != null)
            TransitionToSubState(SubStates.First());
    }
    public void ExitState()
    {
        CurrentSubState?.ExitState();

        OnExitState();

        ParentState = null;
        CurrentSubState = null;
    }
    public void UpdateState()
    {
        OnUpdateState();

        CheckSwitchState();

        if (m_IsTransitionSubStateState == false)
            CurrentSubState?.UpdateState();
    }

    private void SetParentSubState(EState newParentState)
    {
        StateBase<StatesMachine, EState> newStateObject = StateMachine.GetStateObject(newParentState);
        ParentState = newStateObject;
    }
    private void TransitionToSubState(EState newSubState)
    {
        if (SubStates.Contains(newSubState) == false) {
            Debug.LogError($"newState enum '{newSubState}' not fount in the SubStates dictionary {SubStates} \n Add to dictionary or check calling method");
            return;
        }
        if (CurrentSubState != null && CurrentSubState.StateKey.Equals(newSubState)) {
            Debug.LogError($"Trying to set '{newSubState}' the same state twice");
            return;
        }

        StateBase<StatesMachine, EState> newStateObject = StateMachine.GetStateObject(newSubState);

        m_IsTransitionSubStateState = true;
        CurrentSubState?.ExitState();

        CurrentSubState = newStateObject;
        CurrentSubState.SetParentSubState(StateKey);

        CurrentSubState.EnterState();
        m_IsTransitionSubStateState = false;
    }
}