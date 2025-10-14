using System;

namespace VE
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