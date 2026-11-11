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
        void CreateInfoBadge()
        {
            infoBadge = new GameObject("InfoBadge", typeof(RectTransform), typeof(Image));
            infoBadge.transform.SetParent(transform, false);
            var bgRect = (RectTransform)infoBadge.transform;
            bgRect.pivot = new Vector2(0, 0);
            bgRect.anchorMin = new Vector2(0, 1);
            bgRect.anchorMax = new Vector2(0, 1);
            bgRect.anchoredPosition = new Vector2(4, 4);
            bgRect.sizeDelta = new Vector2(190, 26);

            var bg = infoBadge.GetComponent<Image>();
            bg.raycastTarget = false;
            bg.color = new Color(0.1f, 0.12f, 0.16f, 0.9f);

            var textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(infoBadge.transform, false);
            var textRect = (RectTransform)textObj.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            infoText = textObj.GetComponent<TextMeshProUGUI>();
            infoText.fontSize = 13;
            infoText.color = new Color(0.9f, 0.95f, 1f, 1f);
            infoText.alignment = TextAlignmentOptions.Center;
            infoText.text = "0 x 0";
        }
    }
}
