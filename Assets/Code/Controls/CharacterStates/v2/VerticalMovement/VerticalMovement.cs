using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVerticalMovement
{
    public CharacterController CharacterController { get; }
    public bool Gravity { get; }
}
public enum eVerticalStates
{
    Grounded, Falling, Jumping
}