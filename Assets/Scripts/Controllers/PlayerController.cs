using UnityEngine;

namespace Checkers.Controllers
{
    public class PlayerController
    {
        public Team CurrentTeam { get; private set; } = Team.Red;
        public bool IsInputLocked { get; private set; }

        public void SwitchTurn()
        {
            CurrentTeam = (CurrentTeam == Team.Red) ? Team.Black : Team.Red;
            Debug.Log($"Turn switched. Now playing: {CurrentTeam}");
        }

        public void LockInput() => IsInputLocked = true;
        public void UnlockInput() => IsInputLocked = false;
    }
}