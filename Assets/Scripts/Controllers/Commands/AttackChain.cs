using Checkers.Units;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Checkers.Controllers.Commands
{
    public class AttackChain
    {
        private readonly Battlefield _battlefield;
        private readonly Unit _attackingUnit;
        private readonly List<Cell> _visitedCells = new();

        public Unit AttackingUnit => _attackingUnit;
        public Cell CurrentPosition => _attackingUnit.CurrentCell;

        public int StepsCount => _visitedCells.Count;

        public AttackChain(Battlefield battlefield, Unit attackingUnit, Cell startingPosition)
        {
            _battlefield = battlefield;
            _attackingUnit = attackingUnit;
            _visitedCells.Add(startingPosition);
        }

        public bool CanContinueAttack(out AttackCommand nextAttack)
        {
            nextAttack = null;

            var possibleAttack = new AttackCommand(_battlefield, _attackingUnit);
            if (possibleAttack.Variants.Any())
            {
                var validCells = new List<Cell>();
                foreach (var cell in possibleAttack.Variants)
                {
                    if (!_visitedCells.Contains(cell))
                    {
                        validCells.Add(cell);
                    }
                }

                if (validCells.Count > 0)
                {
                    nextAttack = new AttackCommand(_battlefield, _attackingUnit);
                    return true;
                }
            }

            return false;
        }

        public void RecordMove(Cell newPosition)
        {
            _visitedCells.Add(newPosition);
        }

        public bool HasVisited(Cell cell) => _visitedCells.Contains(cell);
    }
}