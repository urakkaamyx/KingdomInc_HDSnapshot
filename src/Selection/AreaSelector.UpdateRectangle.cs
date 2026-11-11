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
        void UpdateRectangle()
        {
            var bounds = new SelectionBox(anchor.x, anchor.y, cursor.x, cursor.y);
            rectangle.anchoredPosition = new Vector2(bounds.Left, bounds.Top);
            rectangle.sizeDelta = new Vector2(bounds.Width, bounds.Height);

            if (infoText != null && owner != null)
            {
                float scale = owner.ResolutionScale;
                int pxW = Mathf.RoundToInt(bounds.Width * scale);
                int pxH = Mathf.RoundToInt(bounds.Height * scale);
                infoText.text = $"{bounds.Width}x{bounds.Height} ({pxW}x{pxH} px)";
            }
        }
    }
}
