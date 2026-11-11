using System;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Behaviour.Util;
using Assets.Source.Flow;
using Assets.Source.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using GameInput = Assets.Behaviour.Input.PlayerInput;

namespace KingdomHDSnapshot
{
    public sealed partial class AreaSelector : MonoBehaviour, IDragDropTarget
    {
        public void Cancel()
        {
            if (DragDropManager.Instance != null && ReferenceEquals(DragDropManager.Instance.Target, this))
            {
                DragDropManager.Instance.ClearDragDrop();
            }

            Active = false;
            dragging = false;
            gameObject.SetActive(false);

            if (owner != null)
            {
                owner.SetToolActive(false);
            }
        }
    }
}
