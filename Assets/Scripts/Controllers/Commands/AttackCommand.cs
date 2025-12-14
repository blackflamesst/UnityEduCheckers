using System.Collections.Generic;
using UnityEngine;
using Checkers.Units;

namespace Checkers.Controllers.Commands
{
    public class AttackCommand : IGameplayCommand
    {
        private readonly Battlefield _battlefield;
        private readonly Unit _selectedUnit;
        private readonly List<Cell> _availableCells = new();
        private readonly Dictionary<Cell, Unit> _unitsToKill = new();

        public IEnumerable<Cell> Variants => _availableCells;

        public AttackCommand(Battlefield battlefield, Unit selectedUnit)
        {
            _battlefield = battlefield;
            _selectedUnit = selectedUnit;
            CalculateAvailableAttacks();
        }

        private void CalculateAvailableAttacks()
        {
            _availableCells.Clear();
            _unitsToKill.Clear();

            var attackDirections = new[]
            {
                NeighbourType.ForwardLeft, NeighbourType.ForwardRight,
                NeighbourType.BackwardLeft, NeighbourType.BackwardRight
            };

            if (_selectedUnit.Type == UnitType.Queen)
            {
                CalculateQueenAttacks();
                return;
            }

            foreach (var direction in attackDirections)
            {
                if (_battlefield.TryGet(_selectedUnit.CurrentCell, direction, out Cell adjacentCell))
                {
                    if (adjacentCell.CurrentUnit != null &&
                        adjacentCell.CurrentUnit.Team != _selectedUnit.Team)
                    {
                        if (_battlefield.TryGet(adjacentCell, direction, out Cell targetCell) &&
                            targetCell.CurrentUnit == null)
                        {
                            _availableCells.Add(targetCell);
                            _unitsToKill[targetCell] = adjacentCell.CurrentUnit;
                        }
                    }
                }
            }
        }

        private void CalculateQueenAttacks()
        {
            var directions = new[]
            {
                NeighbourType.ForwardLeft, NeighbourType.ForwardRight,
                NeighbourType.BackwardLeft, NeighbourType.BackwardRight
            };

            foreach (var direction in directions)
            {
                Cell currentCell = _selectedUnit.CurrentCell;
                Unit enemyUnit = null;
                Cell enemyCell = null;

                while (_battlefield.TryGet(currentCell, direction, out Cell nextCell))
                {
                    if (nextCell.CurrentUnit != null)
                    {
                        if (nextCell.CurrentUnit.Team != _selectedUnit.Team && enemyUnit == null)
                        {
                            enemyUnit = nextCell.CurrentUnit;
                            enemyCell = nextCell;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else if (enemyUnit != null)
                    {
                        _availableCells.Add(nextCell);
                        _unitsToKill[nextCell] = enemyUnit;
                    }

                    currentCell = nextCell;
                }
            }
        }

        public void Interact(Cell cell)
        {
            if (!_availableCells.Contains(cell))
            {
                Debug.LogWarning("Невозможно атаковать эту клетку!");
                return;
            }

            if (_unitsToKill.TryGetValue(cell, out Unit enemyUnit))
            {
                enemyUnit.Dead();
                enemyUnit.CurrentCell.CurrentUnit = null;
                enemyUnit.CurrentCell = null;
            }

            _selectedUnit.CurrentCell.CurrentUnit = null;
            _selectedUnit.CurrentCell = cell;
            cell.CurrentUnit = _selectedUnit;
            _selectedUnit.MoveVisuals(cell.transform.position + Vector3.up * 1.1f);

            CheckForPromotion(cell);

            // TODO: проверить возможность последующей атаки (рекурсивные ходы)
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