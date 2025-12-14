using System;
using System.Linq;
using Checkers.Controllers.Commands;
using Checkers.Interfaces;
using Checkers.Units;
using UnityEngine;
using Zenject;

namespace Checkers
{
    public class PlayerController : IInitializable, IDisposable
    {
        private readonly ISharedData _sharedData;
        private readonly Battlefield _battlefield;
        private readonly Controls.GameActions _controls;
        private readonly SignalBus _signalBus;

        private IGameplayCommand _currentCommand;

        public PlayerController(
            ISharedData sharedData,
            Battlefield battlefield,
            Controls.GameActions controls,
            SignalBus signalBus)
        {
            _sharedData = sharedData;
            _battlefield = battlefield;
            _controls = controls;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _battlefield.OnCellClicked += HandleCellClick;

            _controls.Confirm.performed += ctx => HandleConfirm();
            _controls.Cancel.performed += ctx => HandleCancel();

            _sharedData.Status = GameStatus.Select;

            _sharedData.CurrentTurn = Team.Red;
        }

        public void Dispose()
        {
            _battlefield.OnCellClicked -= HandleCellClick;
        }

        private void HandleCellClick(Cell clickedCell)
        {
            if (_sharedData.Lock) return;

            switch (_sharedData.Status)
            {
                case GameStatus.Select:
                    HandleSelection(clickedCell);
                    break;

                case GameStatus.Move:
                    HandleMovement(clickedCell);
                    break;

                case GameStatus.ConfirmMove:
                    HandleConfirmation(clickedCell);
                    break;
                case GameStatus.ConfirmAttack:
                    HandleConfirmation(clickedCell);
                    break;
            }
        }

        private void HandleSelection(Cell clickedCell)
        {
            if (clickedCell.CurrentUnit != null)
            {
                if (clickedCell.CurrentUnit.Team != _sharedData.CurrentTurn)
                {
                    Debug.Log($"Сейчас ходят {_sharedData.CurrentTurn}, нельзя выбрать шашку команды {clickedCell.CurrentUnit.Team}");
                    return;
                }

                _sharedData.Destination = clickedCell.CurrentUnit;

                var attackCommand = new AttackCommand(_battlefield, clickedCell.CurrentUnit);
                if (attackCommand.Variants.Any())
                {
                    _sharedData.Status = GameStatus.Attack;
                    _sharedData.Command = attackCommand;
                }
                else
                {
                    _sharedData.Status = GameStatus.Move;
                    _sharedData.Command = new MoveCommand(_battlefield, clickedCell.CurrentUnit);
                }

                _signalBus.Fire(GameEvent.SelectDestination);
            }
        }

        private void HandleMovement(Cell clickedCell)
        {
            var command = _sharedData.Command;

            if (command == null) return;

            foreach (var cell in command.Variants)
            {
                if (cell == clickedCell)
                {
                    _sharedData.Target = clickedCell;

                    _sharedData.Status = _sharedData.Status == GameStatus.Attack
                    ? GameStatus.ConfirmAttack
                    : GameStatus.ConfirmMove;

                    _signalBus.Fire(GameEvent.SelectTarget);
                    return;
                }
            }

            ClearSelection();
        }

        private void HandleConfirmation(Cell clickedCell)
        {
            if (_controls.Confirm.IsPressed())
            {
                ExecuteMove();
            }
            else
            {
                _sharedData.Target = null;

                _sharedData.Status = _sharedData.Status == GameStatus.ConfirmAttack
                ? GameStatus.Attack
                : GameStatus.Move;

                _signalBus.Fire(GameEvent.SwitchMode);
            }
        }

        private void ExecuteMove()
        {
            _sharedData.Lock = true;
            _sharedData.Status = GameStatus.Lock;
            _signalBus.Fire(GameEvent.VisualisationPeriod);

            _sharedData.Command?.Interact(_sharedData.Target);

            CompleteTurn();
        }

        private void HandleConfirm()
        {
            if (_sharedData.Status == GameStatus.ConfirmMove && _sharedData.Target != null)
            {
                ExecuteMove();
            }
        }

        private void HandleCancel()
        {
            ClearSelection();
        }

        private void ClearSelection()
        {
            _sharedData.Destination = null;
            _sharedData.Target = null;
            _sharedData.Status = GameStatus.Select;
            _sharedData.Command = null;
            _signalBus.Fire(GameEvent.SwitchMode);
        }

        private void CompleteTurn()
        {
            _sharedData.CurrentTurn = _sharedData.CurrentTurn == Team.Red
            ? Team.Black
            : Team.Red;

            Debug.Log($"Теперь ходят: {_sharedData.CurrentTurn}");

            _sharedData.Destination = null;
            _sharedData.Target = null;
            _sharedData.Lock = false;
            _sharedData.Status = GameStatus.Select;
            _sharedData.Command = null;

            _signalBus.Fire(GameEvent.NewTurn);
        }
    }
}