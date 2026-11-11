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
        public void OnDragDropEnd()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasReleasedThisFrame)
            {
                Cancel();
                return;
            }

            if (dragging) RefreshCursor();

            var bounds = new SelectionBox(anchor.x, anchor.y, cursor.x, cursor.y);

            if (!dragging || !bounds.HasArea)
            {
                dragging = false;
                rectangle.sizeDelta = Vector2.zero;
                anchor = cursor = Vector2Int.zero;
                if (Active && DragDropManager.Instance != null)
                {
                    DragDropManager.Instance.SetDragDropTarget(this, false);
                }
                else
                {
                    Cancel();
                }
                return;
            }

            Cancel();
            owner.StartCapture(bounds);
        }
    }
}
