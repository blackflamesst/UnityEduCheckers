using Checkers;
using System.Collections.Generic;

public interface IGameplayCommand
{
    IEnumerable<Cell> Variants { get; }

    void Interact(Cell cell);
}
