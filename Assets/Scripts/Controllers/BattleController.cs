using Checkers.Interfaces;
using UnityEngine;
using Zenject;

namespace Checkers
{
    public class BattleController : MonoBehaviour
    {
        private Controls.GameActions _controls;
        private ISharedData _sharedData;

        [Inject]
        private void Construct(Controls.GameActions controls, ISharedData sharedData)
        {
            _controls = controls;
            _sharedData = sharedData;
        }

    }
}