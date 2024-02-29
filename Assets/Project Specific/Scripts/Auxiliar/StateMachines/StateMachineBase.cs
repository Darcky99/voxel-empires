using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Sirenix.OdinInspector;

[Serializable]
public abstract class StateMachineBase<StateMachine, EState> : IStateMachine where StateMachine : StateMachineBase<StateMachine, EState> where EState : Enum
{
    #region Editor
    [SerializeField] private bool m_ShowDebug = true;

    private bool m_ShowMainState { get => m_ShowDebug && CurrentState != null; }

    [SerializeField, ReadOnly, ShowIf(nameof(m_ShowMainState))] private EState m_CurrentStateKey;
    #endregion


    public StateBase<StateMachine, EState> CurrentState { get; private set; }

    protected bool IsTransitionState { get; private set; }

    private Dictionary<EState, StateBase<StateMachine, EState>> m_States;


    public virtual void Initialize() => TransitionToState(m_States.First().Key);

    public void Update()
    {
        m_CurrentStateKey = CurrentState.StateKey;

        OnUpdate();

        if (IsTransitionState == false)
            CurrentState?.UpdateState();
    }

    #region Physics
    public void OnCollisionEnter(Collision collision) => CurrentState.OnCollisionEnter(collision);
    public void OnCollisionStay(Collision collision) => CurrentState.OnCollisionStay(collision);
    public void OnCollisionExit(Collision collision) => CurrentState.OnCollisionExit(collision);

    public void OnTriggerEnter(Collider other) => CurrentState.OnTriggerEnter(other);
    public void OnTriggerStay(Collider other) => CurrentState.OnTriggerStay(other);
    public void OnTriggerExit(Collider other) => CurrentState.OnTriggerExit(other);
    #endregion

    protected virtual void OnUpdate() { }

    protected void SetStates(params StateBase<StateMachine, EState>[] mainStates)
    {
        m_States = new Dictionary<EState, StateBase<StateMachine, EState>>();

        foreach (StateBase<StateMachine, EState> state in mainStates)
            m_States.Add(state.StateKey, state);
    }

    public StateBase<StateMachine, EState> GetStateObject(EState requestedState)
    {
        StateBase<StateMachine, EState> stateObject = null;

        if (m_States.TryGetValue(requestedState, out stateObject))
            return stateObject;
        else
            Debug.LogError($"The state '{requestedState}' was not fount in the StateMachine dictionary");

        return stateObject;
    }

    public void TransitionToState(EState newState)
    {
        if (m_States.TryGetValue(newState, out StateBase<StateMachine, EState> newStateObject) == false) {
            Debug.LogError($"newState enum '{newState}' not fount in the States dictionary {m_States}. \n Add newState or check calling method");
            return;
        }
        if (CurrentState != null && CurrentState.StateKey.Equals(newState)) {
            Debug.LogError($"Trying to set '{newState}' the same state twice");
            return;
        }

        IsTransitionState = true;
        CurrentState?.ExitState();
        CurrentState = newStateObject;
        CurrentState.EnterState();
        IsTransitionState = false;
    }
}