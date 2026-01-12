using System;

namespace VoxelEmpires
{
    public class GamaStateChangeEventArgs : EventArgs
    {
        public GamaStateChangeEventArgs(GameState current)
        {
            CurrentState = current;
        }

        public GameState CurrentState;
    }
}