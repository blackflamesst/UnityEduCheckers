using UnityEngine;

namespace Checkers.Settings
{

    [CreateAssetMenu(fileName = "New CellPaletteSettings", menuName = "Settings/CellPaletteSettings", order = 52)]
    public class CellPaletteSettings : ScriptableObject
    {
        [field: SerializeField, Space(20f)]
        [field: Tooltip("Клетка под выбранным юнитом")]
        public Material SelectCell { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Клетка доступная для передвижения")]
        public Material MoveCell { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Клетка доступная для атаки")]
        public Material AttackCell { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Клетка доступная для передвижения и атаки")]
        public Material MoveAndAttackCell { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Клетка доступная для передвижения и атаки")]
        public Material ConfirmCell { get; private set; }
    }
}

