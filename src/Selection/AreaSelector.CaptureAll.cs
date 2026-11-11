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
        public void CaptureAll()
        {
            var flow = GamePlayer.Current?.ActiveFlow;
            if (flow == null || WorldFlowManager.Instance == null)
            {
                Cancel();
                return;
            }

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;
            bool foundAny = false;

            foreach (var elem in WorldFlowManager.Instance.Elements)
            {
                if (elem == null || !elem.isActiveAndEnabled) continue;
                foundAny = true;
                minX = Math.Min(minX, elem.Position.x);
                maxX = Math.Max(maxX, elem.Position.x + Mathf.RoundToInt(elem.Size.x));
                minY = Math.Min(minY, elem.Position.y - Mathf.RoundToInt(elem.Size.y));
                maxY = Math.Max(maxY, elem.Position.y);
            }

            if (flow.Decorations != null)
            {
                foreach (var deco in flow.Decorations)
                {
                    foundAny = true;
                    minX = Math.Min(minX, deco.Position.x);
                    maxX = Math.Max(maxX, deco.Position.x + deco.Size.x);
                    minY = Math.Min(minY, deco.Position.y - deco.Size.y);
                    maxY = Math.Max(maxY, deco.Position.y);
                }
            }

            if (!foundAny)
            {
                owner?.ShowNotification("No machines or decorations found to capture.");
                Cancel();
                return;
            }

            // Add 64 units (1 grid block) margin
            var bounds = new SelectionBox(minX - 64, minY - 64, maxX + 64, maxY + 64);
            Cancel();
            owner.StartCapture(bounds);
        }
    }
}
