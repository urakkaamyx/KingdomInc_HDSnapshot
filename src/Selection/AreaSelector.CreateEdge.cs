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
        Image CreateEdge(string name, Vector2 min, Vector2 max, Vector2 size, Color color)
        {
            var edge = new GameObject(name, typeof(RectTransform), typeof(Image));
            edge.transform.SetParent(transform, false);
            var rect = (RectTransform)edge.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            var image = edge.GetComponent<Image>();
            image.raycastTarget = false;
            image.color = color;
            return image;
        }
    }
}
