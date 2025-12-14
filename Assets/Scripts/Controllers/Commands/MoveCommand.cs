using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Checkers.Units;
using System.Linq;

namespace Checkers.Controllers.Commands
{
    public class MoveCommand : IGameplayCommand
    {
        private readonly Battlefield _battlefield;
        private readonly Unit _selectedUnit;
        private readonly List<Cell> _availableCells = new();

        public IEnumerable<Cell> Variants => _availableCells;

        public bool HasAvailableMoves => _availableCells.Count > 0;

        private bool _isAttack;

        public MoveCommand(Battlefield battlefield, Unit selectedUnit)
        {
            _battlefield = battlefield;
            _selectedUnit = selectedUnit;
            CalculateAvailableMoves();
        }

        private void CalculateAvailableMoves()
        {
            _availableCells.Clear();

            var attackCommand = new AttackCommand(_battlefield, _selectedUnit);
            if (attackCommand.Variants.Any())
            {
                _availableCells.AddRange(attackCommand.Variants);
                _isAttack = true;
                return;
            }

            _isAttack = false;

            bool isRed = _selectedUnit.Team == Team.Red;

            var forwardDirections = isRed
                ? new[] { NeighbourType.ForwardLeft, NeighbourType.ForwardRight }
                : new[] { NeighbourType.BackwardLeft, NeighbourType.BackwardRight };

            if (_selectedUnit.Type == UnitType.Queen)
            {
                forwardDirections = new[]
                {
                    NeighbourType.ForwardLeft, NeighbourType.ForwardRight,
                    NeighbourType.BackwardLeft, NeighbourType.BackwardRight
                };
            }

            foreach (var direction in forwardDirections)
            {
                if (_battlefield.TryGet(_selectedUnit.CurrentCell, direction, out Cell targetCell))
                {
                    if (targetCell.CurrentUnit == null)
                    {
                        _availableCells.Add(targetCell);
                    }
                    // TODO: позже добавим логику дл€ атаки через клетку
                }
            }
        }

        public void Interact(Cell cell)
        {
            if (!_availableCells.Contains(cell))
            {
                Debug.LogWarning("Ќевозможно переместитьс€ на эту клетку!");
                return;
            }

            if (_isAttack)
            {
                var attackCommand = new AttackCommand(_battlefield, _selectedUnit);
                attackCommand.Interact(cell);
            }
            else
            {
                _selectedUnit.CurrentCell.CurrentUnit = null;
                _selectedUnit.CurrentCell = cell;
                cell.CurrentUnit = _selectedUnit;
                _selectedUnit.MoveVisuals(cell.transform.position + Vector3.up * 1.1f);
                CheckForPromotion(cell);
            }
        }

        private void CheckForPromotion(Cell cell)
        {
            bool isRed = _selectedUnit.Team == Team.Red;
            bool isLastRow = isRed
                ? IsLastRowForRed(cell)
                : IsLastRowForBlack(cell);

            if (isLastRow && _selectedUnit.Type != UnitType.Queen)
            {
                _selectedUnit.PromoteToQueen();
            }
        }

        private bool IsLastRowForRed(Cell cell)
            => cell.transform.position.z == 7f;
        

        private bool IsLastRowForBlack(Cell cell)
            => cell.transform.position.z == -7f;
    }
}