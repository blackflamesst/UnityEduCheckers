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
            //Debug.Log("[PlayerController] Initialized");
        }

        public void Dispose()
        {
            _battlefield.OnCellClicked -= HandleCellClick;
        }

        private void HandleCellClick(Cell clickedCell)
        {
            if (_sharedData.Lock) return;

            //Debug.Log($"[Click] Cell: {clickedCell.name} | Status: {_sharedData.Status}");

            switch (_sharedData.Status)
            {
                case GameStatus.Select:
                    HandleSelection(clickedCell);
                    break;

                case GameStatus.Move:
                    HandleMovement(clickedCell);
                    break;

                case GameStatus.Attack:
                    HandleMovement(clickedCell);
                    break;

                case GameStatus.ConfirmMove:
                case GameStatus.ConfirmAttack:
                    HandleConfirmation(clickedCell);
                    break;
            }
        }

        private void HandleSelection(Cell clickedCell)
        {
            if (clickedCell.CurrentUnit != null)
            {
                if (clickedCell.CurrentUnit.Team != _sharedData.CurrentTurn) return;

                if (_sharedData.CurrentAttackChain != null &&
                    clickedCell.CurrentUnit != _sharedData.CurrentAttackChain.AttackingUnit) return;

                _sharedData.Destination = clickedCell.CurrentUnit;

                AttackCommand attackCommand;
                if (_sharedData.CurrentAttackChain != null)
                    attackCommand = new AttackCommand(_battlefield, clickedCell.CurrentUnit, _sharedData.CurrentAttackChain);
                else
                    attackCommand = new AttackCommand(_battlefield, clickedCell.CurrentUnit);

                if (attackCommand.Variants.Any())
                {
                    //Debug.Log($"[Selection] Found Attack variants: {attackCommand.Variants.Count()}");
                    _sharedData.Status = GameStatus.Attack;
                    _sharedData.Command = attackCommand;

                    if (_sharedData.CurrentAttackChain == null)
                        _sharedData.CurrentAttackChain = new AttackChain(_battlefield, clickedCell.CurrentUnit, clickedCell);
                }
                else if (_sharedData.CurrentAttackChain == null)
                {
                    var moveCommand = new MoveCommand(_battlefield, clickedCell.CurrentUnit);
                    if (moveCommand.Variants.Any())
                    {
                        //Debug.Log($"[Selection] Found Move variants: {moveCommand.Variants.Count()}");
                        _sharedData.Status = GameStatus.Move;
                        _sharedData.Command = moveCommand;
                    }
                    else
                    {
                        //Debug.Log("[Selection] No moves available");
                        return;
                    }
                }
                else
                {
                    CompleteTurn();
                    return;
                }

                _signalBus.Fire(GameEvent.SelectDestination);
            }
        }

        private void HandleMovement(Cell clickedCell)
        {
            var command = _sharedData.Command;

            if (command == null)
            {
                //Debug.LogError("[HandleMovement] Command is NULL! Resetting selection.");
                ClearSelection();
                return;
            }

            foreach (var cell in command.Variants)
            {
                if (cell == clickedCell)
                {
                    //Debug.Log($"[HandleMovement] Target selected: {clickedCell.name}");
                    _sharedData.Target = clickedCell;

                    _sharedData.Status = _sharedData.Status == GameStatus.Attack
                        ? GameStatus.ConfirmAttack
                        : GameStatus.ConfirmMove;

                    _signalBus.Fire(GameEvent.SelectTarget);
                    return;
                }
            }

            //Debug.Log("[HandleMovement] Clicked cell is not a valid variant.");
            ClearSelection();
        }

        private void HandleConfirmation(Cell clickedCell)
        {
            bool isClickConfirm = clickedCell == _sharedData.Target;
            bool isKeyConfirm = _controls.Confirm.IsPressed();

            //Debug.Log($"[Confirmation] Clicked: {clickedCell.name}, Target: {_sharedData.Target?.name}. Match? {isClickConfirm}");

            if (isKeyConfirm || isClickConfirm)
            {
                //Debug.Log("[Confirmation] Executing move...");
                ExecuteMove();
            }
            else
            {
                //Debug.Log("[Confirmation] Canceled (Clicked different cell). Back to selection.");
                _sharedData.Target = null;

                _sharedData.Status = _sharedData.Status == GameStatus.ConfirmAttack
                    ? GameStatus.Attack
                    : GameStatus.Move;

                _signalBus.Fire(GameEvent.SwitchMode);
            }
        }

        private void ExecuteMove()
        {
            if (_sharedData.Command == null)
            {
                //Debug.LogError("CRITICAL: _sharedData.Command is NULL inside ExecuteMove!");
                return;
            }

            if (_sharedData.Target == null)
            {
                //Debug.LogError("CRITICAL: _sharedData.Target is NULL inside ExecuteMove!");
                return;
            }

            _sharedData.Lock = true;
            _sharedData.Status = GameStatus.Lock;
            _signalBus.Fire(GameEvent.VisualisationPeriod);

            //Debug.Log("[ExecuteMove] Calling Interact...");
            _sharedData.Command.Interact(_sharedData.Target);

            if (_sharedData.CurrentAttackChain != null)
            {
                CheckForChainContinuation();
            }
            else
            {
                CompleteTurn();
            }
        }

        private void CheckForChainContinuation()
        {
            if (_sharedData.CurrentAttackChain.CanContinueAttack(out AttackCommand nextAttack))
            {
                _sharedData.Lock = false;
                _sharedData.Status = GameStatus.Attack;
                _sharedData.Command = nextAttack;
                _sharedData.Target = null;
                //Debug.Log("Chain continue available");
                _signalBus.Fire(GameEvent.SwitchMode);
            }
            else
            {
                _sharedData.CurrentAttackChain = null;
                CompleteTurn();
            }
        }

        private void HandleConfirm()
        {
            bool isConfirmState = _sharedData.Status == GameStatus.ConfirmMove ||
                                  _sharedData.Status == GameStatus.ConfirmAttack;

            if (isConfirmState && _sharedData.Target != null)
            {
                //Debug.Log("[HandleConfirm] Key Pressed. Executing.");
                ExecuteMove();
            }
        }

        private void HandleCancel()
        {
            if (_sharedData.CurrentAttackChain != null && _sharedData.CurrentAttackChain.StepsCount > 1)
            {
                CompleteTurn();
            }
            else
            {
                ClearSelection();
            }
        }

        private void ClearSelection()
        {
            //Debug.Log("[ClearSelection]");
            _sharedData.Destination = null;
            _sharedData.Target = null;
            _sharedData.Status = GameStatus.Select;
            _sharedData.Command = null;
            _sharedData.CurrentAttackChain = null;

            _signalBus.Fire(GameEvent.SwitchMode);
        }

        private void CompleteTurn()
        {
            _sharedData.CurrentTurn = _sharedData.CurrentTurn == Team.Red
                ? Team.Black
                : Team.Red;

            //Debug.Log($"[CompleteTurn] New Turn: {_sharedData.CurrentTurn}");

            _sharedData.Destination = null;
            _sharedData.Target = null;
            _sharedData.Lock = false;
            _sharedData.Status = GameStatus.Select;
            _sharedData.Command = null;
            _sharedData.CurrentAttackChain = null;

            _signalBus.Fire(GameEvent.NewTurn);
        }
    }
}