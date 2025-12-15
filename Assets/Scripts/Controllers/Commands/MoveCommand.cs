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

        public MoveCommand(Battlefield battlefield, Unit selectedUnit)
        {
            _battlefield = battlefield;
            _selectedUnit = selectedUnit;
            CalculateAvailableMoves();
        }

        private void CalculateAvailableMoves()
        {
            _availableCells.Clear();

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
                if (_selectedUnit.Type == UnitType.Queen)
                {
                    Cell current = _selectedUnit.CurrentCell;
                    while (_battlefield.TryGet(current, direction, out Cell targetCell))
                    {
                        if (targetCell.CurrentUnit == null)
                        {
                            _availableCells.Add(targetCell);
                            current = targetCell;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    if (_battlefield.TryGet(_selectedUnit.CurrentCell, direction, out Cell targetCell))
                    {
                        if (targetCell.CurrentUnit == null)
                        {
                            _availableCells.Add(targetCell);
                        }
                    } 
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

                _selectedUnit.CurrentCell.CurrentUnit = null;
                _selectedUnit.CurrentCell = cell;
                cell.CurrentUnit = _selectedUnit;
                _selectedUnit.MoveVisuals(cell.transform.position + Vector3.up * 1.1f);
                CheckForPromotion(cell);
        }

        private void CheckForPromotion(Cell cell)
        {
            bool isRed = _selectedUnit.Team == Team.Red;
            bool isLastRow = isRed ? cell.transform.position.z >= 7f : cell.transform.position.z <= -7f;

            if (isLastRow && _selectedUnit.Type != UnitType.Queen)
            {
                _selectedUnit.PromoteToQueen();
            }
        }
    }
}