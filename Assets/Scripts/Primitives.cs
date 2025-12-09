using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{

    public enum Team
    {
        Red, Black
    }

    [System.Flags]
    public enum UnitType
    {
        Default = 0,
        Queen = 1
    }

    public enum Movetype
    {
        None, 
        Normal, 
        Kill
    }

    public enum GameEvent
    {
        Empty = 0,

        Select = 1,

        Cancel = 2,

        Confirm = 3
    }

    public enum GameStatus
    {
        Error = 0,
#region Глобальные состояние: [0 - 9]
        Lock = 1,
        Unlock = 2,
#endregion

#region Управление игрока: [10+]
        Select = 3,
        Move = 4,
        Attack = 5,
        Confirm = 6, 
        Cancel = 7
#endregion
    }

    public enum NeighbourType
    {
        Forward = 0,
        Backward = 1,
        Right = 2,
        Left = 3,
        ForwardRight = 4,
        ForwardLeft = 5,
        BackwardRight = 6,
        BackwardLeft = 7
    }
}