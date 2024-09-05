using Utilities.StateMachine;

public class GroundMovement_StateMachine : StateMachineBase<GroundMovement_StateMachine, eGroundStates>
{
    public GroundMovement_StateMachine(IGroundMovement iGroundMovement)
    {
        IGroundMovement = iGroundMovement;

        SetStates(
            new GroundMovementState_Still(this),
            new GroundMovement_Moving(this)
            );
    }

    public IGroundMovement IGroundMovement { get; private set; }
}