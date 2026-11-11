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
        HDSnapshotPlugin owner;
        RectTransform rectangle;
        Image fillImage;
        Image topEdge, bottomEdge, leftEdge, rightEdge;
        GameObject infoBadge;
        TMP_Text infoText;

        Vector2Int anchor;
        Vector2Int cursor;
        bool dragging;

        public bool Active { get; private set; }

        public void Initialize(HDSnapshotPlugin plugin, RectTransform nativeTemplate)
        {
            owner = plugin;
            rectangle = (RectTransform)transform;

            if (nativeTemplate != null)
            {
                rectangle.anchorMin = nativeTemplate.anchorMin;
                rectangle.anchorMax = nativeTemplate.anchorMax;
                rectangle.pivot = new Vector2(0, 1); // Top-left pivot
                rectangle.localScale = Vector3.one;
                rectangle.localRotation = Quaternion.identity;
                rectangle.anchoredPosition3D = Vector3.zero;
            }

            // Semi-transparent blueprint blue / cyan fill
            fillImage = gameObject.AddComponent<Image>();
            fillImage.raycastTarget = false;
            fillImage.color = new Color(0.2f, 0.55f, 0.95f, 0.12f);

            // Crisp colored borders
            Color borderColor = new Color(0.35f, 0.75f, 1f, 0.95f);
            topEdge = CreateEdge("Top", new Vector2(0, 1), Vector2.one, new Vector2(0, 2), borderColor);
            bottomEdge = CreateEdge("Bottom", Vector2.zero, new Vector2(1, 0), new Vector2(0, 2), borderColor);
            leftEdge = CreateEdge("Left", Vector2.zero, new Vector2(0, 1), new Vector2(2, 0), borderColor);
            rightEdge = CreateEdge("Right", new Vector2(1, 0), Vector2.one, new Vector2(2, 0), borderColor);

            // Floating dimensions badge
            CreateInfoBadge();

            gameObject.SetActive(false);
        }
    }
}
