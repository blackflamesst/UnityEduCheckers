using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers.Units
{
    public class Unit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [field: SerializeField] public Team Team {  get; private set; }
        [field: SerializeField] public UnitType Type {  get; private set; } = UnitType.Default;

        public Cell CurrentCell { get; set; }

        public void PromoteToQueen()
        {
            Type = UnitType.Queen;

            transform.localScale *= 1.2f;
        }

        public void MoveVisuals(Vector3 targetPosition) => transform.position = targetPosition;

        public void Dead() => gameObject.SetActive(false);


        public void OnPointerEnter(PointerEventData eventData)
        {
            if (CurrentCell != null)
            {
                CurrentCell.OnPointerEnter(eventData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (CurrentCell != null)
            {
                CurrentCell.OnPointerExit(eventData);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (CurrentCell != null)
            {
                CurrentCell.OnPointerClick(eventData);
            }
        }
    }
}

