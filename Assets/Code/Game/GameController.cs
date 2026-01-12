using System.Collections;
using UnityEngine;

namespace VoxelEmpires
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] GameManager _gameManager;

        private void Start()
        {
            StartCoroutine(StartGame());
        }

        private IEnumerator StartGame()
        {
            yield return new WaitForSeconds(0.75f);
            _gameManager.OpenWorld();
        }
    }
}