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

        SelectDestination = 1,

        // добавился для визуального изменения подсветки клеток, необходимо, чтобы игрок понимал, что он переключился
        SwitchMode = 2,

        SelectTarget = 3,

        //Отыгрывание нашего кода
        VisualisationPeriod = 4,

        // Событие передачи хода другого игроку
        NewTurn = 5
    }

    public enum GameStatus
    {
        Error = 0,

        Lock = 1,
        
        Select = 2,
        
        Move = 3,
        
        Attack = 4,
        
        ConfirmMove = 5,
        
        ConfirmAttack = 6
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