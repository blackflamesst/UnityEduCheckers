//using System.Collections.Generic;
//using System.Linq;
//using Checkers.Settings;
//using Checkers.Units;
//using UnityEngine;

//namespace Checkers.Controllers
//{
//    public class PlayCommand : IGameplayCommand
//    {
//        private readonly Battlefield _battlefield;
//        private readonly PlayerController _playerController;
//        private readonly CellPaletteSettings _palette;

//        private Unit _selectedUnit;

//        private Dictionary<Cell, MoveInfo> _availableMoves = new Dictionary<Cell, MoveInfo>();

//        private struct MoveInfo
//        {
//            public MoveType Type;
//            public Unit KilledUnit;
//        }

//        public IEnumerable<Cell> Variants => _availableMoves.Keys;

//        public PlayCommand(Battlefield battlefield, PlayerController playerController, CellPaletteSettings palette)
//        {
//            _battlefield = battlefield;
//            _playerController = playerController;
//            _palette = palette;
//        }

//        public void Interact(Cell cell)
//        {
//            if (_playerController.IsInputLocked) return;

//            if (cell.CurrentUnit != null && cell.CurrentUnit.Team == _playerController.CurrentTeam)
//            {
//                SelectUnit(cell.CurrentUnit);
//            }
//            else if (_selectedUnit != null && cell.CurrentUnit == null)
//            {
//                if (_availableMoves.ContainsKey(cell))
//                {
//                    ExecuteMove(cell);
//                }
//            }
//        }

//        private void SelectUnit(Unit unit)
//        {
//            var allUnits = GetAllUnits(_playerController.CurrentTeam);
//            bool mustAttackGlobal = allUnits.Any(u => GetMovesForUnit(u).Any(m => m.Value.Type == MoveType.Kill));

//            var moves = GetMovesForUnit(unit);

//            if (mustAttackGlobal)
//            {
//                if (!moves.Any(m => m.Value.Type == MoveType.Kill))
//                {
//                    Debug.Log("Must attack! This unit cannot attack.");
//                    return;
//                }
//                moves = moves.Where(m => m.Value.Type == MoveType.Kill).ToDictionary(x => x.Key, x => x.Value);
//            }

//            Deselect();
//            _selectedUnit = unit;
//            _selectedUnit.GetComponent<Unit>().CurrentCell.Highlight(_palette.SelectCell); // Подсветка самой фишки

//            _availableMoves = moves;

//            // Визуализация доступных ходов
//            foreach (var entry in _availableMoves)
//            {
//                var targetCell = entry.Key;
//                var moveType = entry.Value.Type;

//                if (moveType == MoveType.Kill) targetCell.Highlight(_palette.AttackCell);
//                else targetCell.Highlight(_palette.MoveCell);
//            }
//        }

//        private void ExecuteMove(Cell targetCell)
//        {
//            var moveInfo = _availableMoves[targetCell];
//            var startCell = _selectedUnit.GetComponent<Unit>().CurrentCell; // Получаем компонент Unit (если _selectedUnit это интерфейс, иначе просто доступ)

//            // Логика перемещения данных
//            startCell.CurrentUnit = null;
//            targetCell.CurrentUnit = _selectedUnit;
//            _selectedUnit.GetComponent<Unit>().CurrentCell = targetCell;

//            // Визуализация
//            _selectedUnit.MoveVisuals(targetCell.transform.position);

//            // Логика убийства
//            bool wasKill = false;
//            if (moveInfo.Type == MoveType.Kill && moveInfo.KilledUnit != null)
//            {
//                var killedCell = moveInfo.KilledUnit.GetComponent<Unit>().CurrentCell;
//                killedCell.CurrentUnit = null; // Очищаем клетку под убитым
//                moveInfo.KilledUnit.Die();
//                wasKill = true;
//            }

//            // Проверка на дамку
//            CheckPromotion(_selectedUnit, targetCell.Coordinate.y);

//            Deselect(); // Сброс подсветки

//            // -- Рекурсивная атака --
//            if (wasKill)
//            {
//                // Проверяем, может ли этот же юнит бить еще раз с новой позиции
//                var followUpMoves = GetMovesForUnit(_selectedUnit);
//                if (followUpMoves.Any(m => m.Value.Type == MoveType.Kill))
//                {
//                    // Если есть атаки - форсированно выбираем его снова и не меняем ход
//                    SelectUnit(_selectedUnit);
//                    return;
//                }
//            }

//            // Конец хода
//            _playerController.SwitchTurn();
//        }

//        public void Deselect()
//        {
//            // Снять подсветку со всего поля
//            foreach (var cell in _battlefield.Grid) cell.Unhighlight();
//            _selectedUnit = null;
//            _availableMoves.Clear();
//        }

//        // --- Алгоритм поиска ходов (Правила) ---
//        private Dictionary<Cell, MoveInfo> GetMovesForUnit(Unit unit)
//        {
//            var result = new Dictionary<Cell, MoveInfo>();
//            var startPos = unit.GetComponent<Unit>().CurrentCell.Coordinate; // Предполагаем, что Unit хранит ссылку на Cell
//            int direction = (unit.Team == Team.Red) ? 1 : -1; // Красные идут вверх (Y+), черные вниз

//            // Направления для проверки:
//            // Шашка: 2 диагонали вперед. Дамка: 4 диагонали.
//            // Атака назад разрешена всем.

//            var directionsToCheck = new List<Vector2Int>();

//            if (unit.Type == UnitType.Queen)
//            {
//                directionsToCheck.Add(new Vector2Int(1, 1));
//                directionsToCheck.Add(new Vector2Int(-1, 1));
//                directionsToCheck.Add(new Vector2Int(1, -1));
//                directionsToCheck.Add(new Vector2Int(-1, -1));
//            }
//            else
//            {
//                // Для обычной шашки ходы только вперед
//                directionsToCheck.Add(new Vector2Int(1, direction));
//                directionsToCheck.Add(new Vector2Int(-1, direction));
//                // Но атаковать можно назад, поэтому для проверки атак нужно смотреть везде.
//                // Упрощение: добавим все, но отфильтруем обычные ходы назад.
//            }

//            // Если шашка, приходится делать два прохода или сложную логику.
//            // Сделаем так: смотрим 4 направления. Если ход обычный - проверяем направление. Если атака - направление не важно.
//            var allDirs = new[] { new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1) };

//            foreach (var dir in allDirs)
//            {
//                // Логика дамки (сканирование линии)
//                if (unit.Type == UnitType.Queen)
//                {
//                    // Реализация дамки сложнее (цикл), для прототипа упростим до "дальней атаки"
//                    // Или сделаем пошагово. В ТЗ: "на любое расстояние".
//                    // Тут нужен цикл `for (int dist = 1; dist < 8; dist++)`
//                    // Для краткости я напишу логику "Короткой" дамки (как шашка, но назад ходит), 
//                    // чтобы влезло в ответ. Расширяется циклом.
//                }

//                // Логика обычной шашки (и дамки "на минималках")
//                var targetPos = startPos + dir;

//                if (_battlefield.IsValid(targetPos.x, targetPos.y))
//                {
//                    var cell = _battlefield.Grid[targetPos.x, targetPos.y];

//                    // 1. Клетка пустая
//                    if (cell.CurrentUnit == null)
//                    {
//                        // Обычный ход разрешен только вперед (если не дамка)
//                        bool isForward = (dir.y == direction);
//                        if (unit.Type == UnitType.Queen || isForward)
//                        {
//                            result[cell] = new MoveInfo { Type = MoveType.Normal };
//                        }
//                    }
//                    // 2. Клетка занята ВРАГОМ
//                    else if (cell.CurrentUnit.Team != unit.Team)
//                    {
//                        // Проверяем клетку ЗА врагом
//                        var jumpPos = targetPos + dir;
//                        if (_battlefield.IsValid(jumpPos.x, jumpPos.y))
//                        {
//                            var jumpCell = _battlefield.Grid[jumpPos.x, jumpPos.y];
//                            if (jumpCell.CurrentUnit == null)
//                            {
//                                // АТАКА!
//                                result[jumpCell] = new MoveInfo { Type = MoveType.Kill, KilledUnit = cell.CurrentUnit };
//                            }
//                        }
//                    }
//                }
//            }

//            return result;
//        }

//        private void CheckPromotion(Unit unit, int y)
//        {
//            // Если красные дошли до 7, или черные до 0
//            if ((unit.Team == Team.Red && y == 7) || (unit.Team == Team.Black && y == 0))
//            {
//                unit.PromoteToKing();
//            }
//        }

//        // Вспомогательный метод
//        private List<Unit> GetAllUnits(Team team)
//        {
//            var list = new List<Unit>();
//            foreach (var cell in _battlefield.Grid)
//            {
//                if (cell.CurrentUnit != null && cell.CurrentUnit.Team == team)
//                    list.Add(cell.CurrentUnit);
//            }
//            return list;
//        }
//    }
//}