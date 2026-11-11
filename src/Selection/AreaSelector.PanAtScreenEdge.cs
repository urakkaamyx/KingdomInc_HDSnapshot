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
        void PanAtScreenEdge()
        {
            var mouse = Mouse.current;
            if (!Application.isFocused || mouse == null || !WorldFlowCamera.Instance || !Camera.main) return;

            Vector2 mousePos = mouse.position.ReadValue();
            float margin = Mathf.Clamp(Screen.height * 0.04f, 28, 72);

            float dirX = 0f;
            if (mousePos.x < margin) dirX = -1f;
            else if (mousePos.x > Screen.width - margin) dirX = 1f;

            float dirY = 0f;
            if (mousePos.y < margin) dirY = -1f;
            else if (mousePos.y > Screen.height - margin) dirY = 1f;

            if (dirX != 0f || dirY != 0f)
            {
                float unitsPerPixel = 2f * Camera.main.orthographicSize / Mathf.Max(1f, Screen.height);
                Vector2 move = new Vector2(dirX, dirY).normalized * (500f * unitsPerPixel * Mathf.Min(Time.unscaledDeltaTime, 0.05f));
                WorldFlowCamera.Instance.Move(move.x, move.y);
            }
        }
    }
}
