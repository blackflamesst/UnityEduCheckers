using UnityEngine;
using System.Linq;
using Checkers.Units;
using System;
using Checkers.Settings;
using UnityEditor.ShortcutManagement;
using Checkers.Interfaces;
using System.Collections.Generic;
using UnityEngine.ParticleSystemJobs;
using UnityEngine.Timeline;
using Zenject;
using System.Resources;
using UnityEngine.Rendering;
using Unity.VisualScripting;

namespace Checkers
{
    public class Battlefield : IDisposable
    {
        private IGameplayCommand _command;
        private readonly CellPaletteSettings _palettes;
        private readonly ISharedData _data;

        private readonly Dictionary<CellNeighbour, Cell> _neighbours;
        private readonly Cell[] _cells;

        public event Action<Cell> OnCellClicked;

        private void OnCellHandler(Cell cell)
        {
            OnCellClicked?.Invoke(cell);
        }

        public bool TryGet(Cell source, NeighbourType type, out Cell cell)
        {
            var data = new CellNeighbour(type, source);
            return _neighbours.TryGetValue(data, out cell);
        }

        private void Callback()
        {
            foreach (var cell in _cells) 
                cell.ResetSelect();

            if (_data.Destination != null)
                _data.Destination.CurrentCell.SetSelect(_palettes.SelectCell);

            var mat = _data.Status switch
            {
                GameStatus.Move => _palettes.MoveCell,
                GameStatus.Attack => _palettes.AttackCell,
                _ => default(Material)
            };

            if (mat != null)
                foreach (var cell in _command.Variants)
                    cell.SetSelect(mat);

            if (_data.Target != null)
                _data.Destination.CurrentCell.SetSelect(_palettes.ConfirmCell);
        }

        public Battlefield(SignalBus signal, ISharedData data, CellPaletteSettings palettes)
        {
#region Find and Init
            _cells = UnityEngine.Object.FindObjectsOfType<Cell>();
            _neighbours = new Dictionary<CellNeighbour, Cell>(_cells.Length * 8);
            var positions = Array.ConvertAll(_cells, t => t.transform.position);
            var distance = 0f;
            for (int i = 0, iMax = _cells.Length; i < iMax; i++)
            {
                _cells[i].OnPointerClickEvent += OnCellHandler;
#if UNITY_EDITOR
                _cells[i].OnPointerClickEvent += DebugOnPointerClick;
#endif

                for (int j = 0, jMax = _cells.Length; j < jMax; j++)
                {
                    if (i == j) continue;

                    var source = positions[i];
                    var destination = positions[j];

                    var forward = destination.z.CompareTo(source.z);
                    var right = destination.x.CompareTo(source.x);
                    var type = (forward, right) switch
                    {
                        (1, 1) => NeighbourType.ForwardRight,
                        (1, 0) => NeighbourType.Forward,
                        (1, -1) => NeighbourType.ForwardLeft,
                        (0, 1) => NeighbourType.Right,
                        (0, -1) => NeighbourType.Left,
                        (-1, 1) => NeighbourType.BackwardRight,
                        (-1, 0) => NeighbourType.Backward,
                        (-1, -1) => NeighbourType.BackwardLeft,
                        _ => default
                    };
                    var key = new CellNeighbour(type, _cells[i]);
                    var check = _neighbours.TryGetValue(key, out var cell)
                        ? Vector3.Distance(source, cell.transform.position)
                        : float.MaxValue;

                    distance = Vector3.Distance(source, destination);
                    if (distance < check)
                        _neighbours[key] = _cells[j];
                }
            }

            var units = UnityEngine.Object.FindObjectsOfType<Units.Unit>();
            for (int i = 0, iMax = units.Length, index; i < iMax; i++)
            {
                (distance, index) = (float.MaxValue, -1);
                var position = units[i].transform.position;
                for (int j = 0,  jMax = units.Length; j < jMax; j++)
                {
                    var calc = Vector3.Distance(position, positions[j]);
                    if (calc < distance)
                        (distance, index) = (calc, j);
                }

                units[i].CurrentCell = _cells[index];
                _cells[index].CurrentUnit = units[i];
            }

#endregion

            (_data, _palettes) = (data, palettes);
            signal.Subscribe<GameEvent>(Callback);

        }

        public void Dispose()
        {
            for (int i = 0, iMax = _cells.Length; i < iMax; i++)
            {
                _cells[i].OnPointerClickEvent -= OnCellHandler;
#if UNITY_EDITOR
                _cells[i].OnPointerClickEvent -= DebugOnPointerClick;
#endif
            }
        }


#if UNITY_EDITOR

        private void DebugOnPointerClick(Cell cell)
        {
            var start = cell.transform.position;
            Debug.DrawLine(start, start + Vector3.up * 5f, Color.green, 5f);
            
        }
#endif

        private readonly struct CellNeighbour : IEquatable<CellNeighbour>
        {
            private readonly NeighbourType _type;
            private readonly Cell _value;

            public CellNeighbour(NeighbourType type, Cell value)
                => (_type, _value) = (type, value);

            public bool Equals(CellNeighbour other)
                => _type == other._type && Equals(_value, other._value);

            public override bool Equals(object obj)
                => obj is CellNeighbour other && Equals(other);

            public override int GetHashCode()
                => unchecked(HashCode.Combine(_type, _value) - 13);
        }
    }
}