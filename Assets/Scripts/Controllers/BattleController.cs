//using System;
//using Checkers.Settings;
//using UnityEngine;
//using Zenject;

//namespace Checkers.Controllers
//{
//    public class BattleController : IInitializable, IDisposable, ITickable
//    {
//        private readonly Battlefield _battlefield;
//        private readonly PlayCommand _gameplayCommand;

//        private float _restartTimer;
//        private const float RestartTime = 2f;

//        public BattleController(Battlefield battlefield, PlayCommand gameplayCommand)
//        {
//            _battlefield = battlefield;
//            _gameplayCommand = gameplayCommand;
//        }

//        public void Initialize()
//        {
//            _battlefield.Initialize();

//            foreach (var cell in _battlefield.Grid)
//            {
//                cell.OnPointerClickEvent += OnCellClicked;
//            }
//        }

//        public void Dispose()
//        {
//            foreach (var cell in _battlefield.Grid)
//            {
//                if (cell) cell.OnPointerClickEvent -= OnCellClicked;
//            }
//        }

//        private void OnCellClicked(Cell cell)
//        {
//            _gameplayCommand.Interact(cell);
//        }

//        public void Tick()
//        {
//            if (Input.GetKeyDown(KeyCode.Escape))
//            {
//                _gameplayCommand.Deselect();
//            }

//            if (Input.GetKey(KeyCode.Tab))
//            {
//                _restartTimer += Time.deltaTime;
//                Debug.Log($"Restarting... {_restartTimer}");
//                if (_restartTimer >= RestartTime)
//                {
//                    UnityEngine.SceneManagement.SceneManager.LoadScene(
//                        UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
//                }
//            }
//            else
//            {
//                _restartTimer = 0;
//            }
//        }
//    }
//}