using System;
using System.IO;
using System.Reflection;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Behaviour.UI.BuyMenu;
using Assets.Behaviour.Util;
using Assets.Source.Player;
using Behaviour.UI.Tooltip;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using KingdomFoundation.Api;

namespace KingdomHDSnapshot
{
    public sealed partial class HDSnapshotPlugin : BaseUnityPlugin
    {
        void Attach(GameUI ui)
        {
            CleanupUi();

            var originalButton = GetPrivateField<GameObject>(ui, "_cloneMenuButton");
            var editHotbar = GetPrivateField<RectTransform>(ui, "_editHotbar");
            var cloneGhost = GetPrivateField<AreaCopyGhost>(ui, "_cloneGhost");

            if (!originalButton || !editHotbar) return;

            attachedUi = ui;

            // Create toolbar button
            toolbarButton = Instantiate(originalButton, editHotbar, false);
            toolbarButton.name = "HDSnapshotButton";

            buttonImage = toolbarButton.transform.Find("Image")?.GetComponent<Image>();
            if (buttonImage && cameraIcon)
            {
                buttonImage.sprite = cameraIcon;
                buttonImage.type = Image.Type.Simple;
                buttonImage.preserveAspect = true;
                buttonImage.color = Color.white;
            }

            var nativeTooltip = toolbarButton.GetComponent<KeyBindingTooltip>();
            if (nativeTooltip) Destroy(nativeTooltip);

            var tutorial = toolbarButton.transform.Find("TutorialArrow");
            if (tutorial) Destroy(tutorial.gameObject);

            var button = toolbarButton.GetComponent<Button>();
            if (button)
            {
                // Assign a fresh ButtonClickedEvent to clear any serialized persistent listeners from _cloneMenuButton
                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(() =>
                {
                    UISounds.Button();
                    ToggleTool();
                });
            }

            // Create AreaSelector on overlay parent
            if (cloneGhost != null && cloneGhost.transform.parent != null)
            {
                var selectorObj = new GameObject("HDSnapshotSelector", typeof(RectTransform));
                selectorObj.transform.SetParent(cloneGhost.transform.parent, false);
                areaSelector = selectorObj.AddComponent<AreaSelector>();
                areaSelector.Initialize(this, (RectTransform)cloneGhost.transform);
            }

            Logger.LogInfo("HD Snapshot toolbar button attached.");
        }
    }
}
