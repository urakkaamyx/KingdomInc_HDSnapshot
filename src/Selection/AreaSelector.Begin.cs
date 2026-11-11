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
        public void Begin()
        {
            if (!WorldFlowManager.Visible || HDCaptureEngine.IsCapturing) return;

            Active = true;
            dragging = false;
            RefreshCursor();
            anchor = cursor;
            rectangle.sizeDelta = Vector2.zero;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            if (owner != null)
            {
                owner.SetToolActive(true);
            }

            // Register with DragDropManager so the game stops camera map-panning on left-click drag
            if (DragDropManager.Instance != null)
            {
                if (!DragDropManager.Instance.SetDragDropTarget(this, false))
                {
                    Cancel();
                }
            }
        }
    }
}
