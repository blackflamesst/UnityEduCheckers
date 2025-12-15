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

        private readonly AttackChain _attackChain;

        public AttackCommand(Battlefield battlefield, Unit selectedUnit, AttackChain chain = null)
        {
            _battlefield = battlefield;
            _selectedUnit = selectedUnit;
            CalculateAvailableAttacks();
            _attackChain = chain;
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
                CalculateQueenAttacks(attackDirections);
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

        private void CalculateQueenAttacks(NeighbourType[] directions)
        {
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
                        if (_attackChain == null || !_attackChain.HasVisited(nextCell))
                        {
                            _availableCells.Add(nextCell);
                            _unitsToKill[nextCell] = enemyUnit;
                        }
                    }

                    currentCell = nextCell;
                }
            }
        }

        public void Interact(Cell cell)
        {
            Debug.Log($"Попытка атаки на клетку {cell.name} (Position: {cell.transform.position})");

            if (!_availableCells.Contains(cell))
            {
                Debug.LogWarning("Невозможно атаковать эту клетку!");
                return;
            }

            if (_unitsToKill.TryGetValue(cell, out Unit enemyUnit))
            {
                Debug.Log($"Враг найден: {enemyUnit.name}. Уничтожаем.");
                enemyUnit.Dead();
                enemyUnit.CurrentCell.CurrentUnit = null;
                enemyUnit.CurrentCell = null;
            }
            else
            {
                Debug.LogError("ОШИБКА: В словаре _unitsToKill не найден враг для этой клетки!");
            }

            _selectedUnit.CurrentCell.CurrentUnit = null;
            _selectedUnit.CurrentCell = cell;
            cell.CurrentUnit = _selectedUnit;
            _selectedUnit.MoveVisuals(cell.transform.position + Vector3.up * 1.1f);

            Debug.Log("Шашка перемещена визуально и логически.");

            _attackChain?.RecordMove(cell);

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