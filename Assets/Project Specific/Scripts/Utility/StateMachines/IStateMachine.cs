using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStateMachine
{
    public abstract void Initialize();
    public abstract void Update();
    public abstract void OnCollisionEnter(Collision collision);
    public abstract void OnCollisionStay(Collision collision);
    public abstract void OnCollisionExit(Collision collision);
    public abstract void OnTriggerEnter(Collider other);
    public abstract void OnTriggerStay(Collider other);
    public abstract void OnTriggerExit(Collider other);
}