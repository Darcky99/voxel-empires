using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGroundMovement
{
    public CharacterController CharacterController { get; }
    public Transform Camera { get; }
    public float GroundSpeed { get; }

}