using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stateless;
using System;

namespace VE
{
    public class GameManager : Singleton<GameManager>
    {
        public event EventHandler<GamaStateChangeEventArgs> StateChanged;

        public GameState CurrentState => _fsm.State;

        private StateMachine<GameState, GameTrigger> _fsm;

        protected override void OnAwakeEvent()
        {
            _fsm = new StateMachine<GameState, GameTrigger>(GameState.Startup);
            _fsm.Configure(GameState.Startup)
            .Permit(GameTrigger.OpenWorld, GameState.World);
            _fsm.OnTransitionCompleted(OnTransitionCompleted);
        }

        private void OnTransitionCompleted(StateMachine<GameState, GameTrigger>.Transition transition)
        {
            StateChanged?.Invoke(this, new GamaStateChangeEventArgs(transition.Destination));
        }

        public void OpenWorld()
        {
            _fsm.Fire(GameTrigger.OpenWorld);
        }
    }
}
