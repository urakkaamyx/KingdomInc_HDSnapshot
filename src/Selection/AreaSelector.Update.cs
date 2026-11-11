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
        void Update()
        {
            if (!Active || HDCaptureEngine.IsCapturing) return;

            if (!WorldFlowManager.Visible || GameUI.MenuVisible)
            {
                Cancel();
                return;
            }

            // Cancel on Escape or Right click
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cancel();
                return;
            }

            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                Cancel();
                return;
            }

            // Shortcut: Space / Enter to Capture Entire Factory
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            {
                CaptureAll();
                return;
            }

            // Start dragging when mouse is clicked
            if (!dragging && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                foreach (var ui in UIHelper.GetAllMouseOverUI())
                {
                    if (!ui.GetComponentInParent<WorldFlowElement>() &&
                        !ui.GetComponentInParent<WorldFlowDeco>() &&
                        !ui.GetComponentInParent<WorldFlowPipe>())
                    {
                        return;
                    }
                }

                RefreshCursor();
                anchor = cursor;
                dragging = true;
            }
        }
    }
}
