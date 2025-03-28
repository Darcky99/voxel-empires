using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachinesHandler : MonoBehaviour
{
    /// <summary>
    /// Use SetStateMachines() to initialize this object.
    /// </summary>
    protected abstract void Awake();

    protected virtual void Update()
    {
        foreach (IStateMachine stateMachine in m_StateMachines)
            stateMachine.Update();
    }

    #region Physiscs
    private void OnCollisionEnter(Collision collision)
    {
        foreach (IStateMachine stateMachine in m_StateMachines)
            stateMachine.OnCollisionEnter(collision);
    }
    private void OnCollisionStay(Collision collision)
    {
        foreach (IStateMachine stateMachine in m_StateMachines)
            stateMachine.OnCollisionStay(collision);
    }
    private void OnCollisionExit(Collision collision)
    {
        foreach (IStateMachine stateMachine in m_StateMachines)
            stateMachine.OnCollisionExit(collision);
    }
    private void OnTriggerEnter(Collider other)
    {
        foreach (IStateMachine stateMachine in m_StateMachines)
            stateMachine.OnTriggerEnter(other);
    }
    private void OnTriggerStay(Collider other)
    {
        foreach (IStateMachine stateMachine in m_StateMachines)
            stateMachine.OnTriggerStay(other);
    }
    private void OnTriggerExit(Collider other)
    {
        foreach (IStateMachine stateMachine in m_StateMachines)
            stateMachine.OnTriggerExit(other);
    }
    #endregion

    private IStateMachine[] m_StateMachines;

    protected void SetStateMachines(params IStateMachine[] stateMachines)
    {
        m_StateMachines = stateMachines;

        foreach (IStateMachine stateMachine in m_StateMachines)
            stateMachine.Initialize();
    }
}