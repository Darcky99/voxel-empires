using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Sirenix.OdinInspector;

[Serializable]
public abstract class StateMachineBase<StateMachine, EState> : MonoBehaviour where StateMachine : StateMachineBase<StateMachine, EState> where EState : Enum
{
    #region Editor
    [SerializeField] private bool m_ShowDebug = true;

    private bool m_ShowMainState { get => m_ShowDebug && CurrentState != null; }
    private bool m_ShowSubState { get => m_ShowDebug && CurrentState?.CurrentSubState != null; }

    [SerializeField, ReadOnly, ShowIf(nameof(m_ShowMainState))] private EState m_CurrentMainStateKey;
    [SerializeField, ReadOnly, ShowIf(nameof(m_ShowSubState))] private EState m_CurrentSubStateKey;
    #endregion

    public StateBase<StateMachine, EState> CurrentState { get; private set; }

    protected bool IsTransitionState { get; private set; }

    private Dictionary<EState, StateBase<StateMachine, EState>> m_MainStates;
    private Dictionary<EState, StateBase<StateMachine, EState>> m_SubStates;

    /// <summary>
    /// Use both SetMainStates() and SetSubStates() to populate the states Dictionaries.
    /// </summary>
    protected abstract void Awake();

    /// <summary>
    /// If not overwriten, sets the first value for MainStates as first state.
    /// </summary>
    protected virtual void Start() => TransitionToState(m_MainStates.First().Key);

    private void Update()
    {
        m_CurrentMainStateKey = CurrentState.StateKey;
        m_CurrentSubStateKey = CurrentState.CurrentSubState != null ? CurrentState.CurrentSubState.StateKey : default;

        OnUpdate();

        if (IsTransitionState == false)
            CurrentState?.UpdateState();
    }

    #region Physics
    private void OnCollisionEnter(Collision collision) => CurrentState.OnCollisionEnter(collision);
    private void OnCollisionStay(Collision collision) => CurrentState.OnCollisionStay(collision);
    private void OnCollisionExit(Collision collision) => CurrentState.OnCollisionExit(collision);

    private void OnTriggerEnter(Collider other) => CurrentState.OnTriggerEnter(other);
    private void OnTriggerStay(Collider other) => CurrentState.OnTriggerStay(other);
    private void OnTriggerExit(Collider other) => CurrentState.OnTriggerExit(other);
    #endregion

    protected virtual void OnUpdate() { }

    protected void SetMainStates(params StateBase<StateMachine, EState>[] mainStates)
    {
        m_MainStates = new Dictionary<EState, StateBase<StateMachine, EState>>();

        foreach (StateBase<StateMachine, EState> state in mainStates)
            m_MainStates.Add(state.StateKey, state);
    }
    protected void SetSubStates(params StateBase<StateMachine, EState>[] subStates)
    {
        m_SubStates = new Dictionary<EState, StateBase<StateMachine, EState>>();

        foreach (StateBase<StateMachine, EState> state in subStates)
            m_SubStates.Add(state.StateKey, state);
    }

    public StateBase<StateMachine, EState> GetStateObject(EState requestedState)
    {
        StateBase<StateMachine, EState> stateObject = null;

        if (m_MainStates.TryGetValue(requestedState, out stateObject))
            return stateObject;
        else if (m_SubStates.TryGetValue(requestedState, out stateObject))
            return stateObject;
        else
            Debug.LogError($"The state '{requestedState}' was not fount in the StateMachines dictionaries");

        return stateObject;
    }

    public void TransitionToState(EState newState)
    {
        if (m_MainStates.TryGetValue(newState, out StateBase<StateMachine, EState> newStateObject) == false) {
            Debug.LogError($"newState enum '{newState}' not fount in the States dictionary {m_MainStates}. \n Add newState or check calling method");
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