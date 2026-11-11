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
        public void OnDragDropMove(Vector2Int mousePosition, Vector2Int grabOffset)
        {
            cursor = mousePosition;
            if (!dragging) return;
            UpdateRectangle();
        }
    }
}
