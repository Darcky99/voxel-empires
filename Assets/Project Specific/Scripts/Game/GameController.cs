using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using World;

namespace VE
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private WorldManager _worldManager;

        private void Start()
        {
            _worldManager.Initialize();
        }
    }
}