using Checkers.Units;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Checkers.Interfaces
{
    public interface ISharedData
    {
        bool Lock { get; set; }

        GameEvent Event { get; set; }

        GameStatus Status { get; set; }

        Unit Destination { get; set; }

        Cell Target { get; set; }

        IGameplayCommand Command { get; set; }

        Team CurrentTurn { get; set; }
    }


    public class SharedData : ISharedData
    {
        public bool Lock { get; set; }
        public GameEvent Event { get; set; }
        public GameStatus Status { get; set; }
        public Unit Destination { get; set; }
        public Cell Target { get; set; }

        public IGameplayCommand Command { get; set; }

        public Team CurrentTurn { get; set; } = Team.Red;
    }
}
