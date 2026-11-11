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
        void Update()
        {
            var ui = GameUI.Instance;
            if (!ui || FoundationApi.World.Player == null)
            {
                if (toolbarButton || areaSelector) CleanupUi();
                return;
            }

            if (ui != attachedUi || !toolbarButton)
            {
                if (Time.unscaledTime < nextAttachTime) return;
                nextAttachTime = Time.unscaledTime + 2f;
                try
                {
                    Attach(ui);
                }
                catch (Exception ex)
                {
                    CleanupUi();
                    Logger.LogError($"HD Snapshot toolbar attach error: {ex.Message}");
                }
            }

            if (toolbarButton)
            {
                bool workshopVisible = WorldFlowManager.Visible;
                toolbarButton.SetActive(workshopVisible);
                var btn = toolbarButton.GetComponent<Button>();
                if (btn) btn.interactable = workshopVisible && !GameUI.MenuVisible && !HDCaptureEngine.IsCapturing;
            }

            // Hotkey check
            if (Keyboard.current != null && HotkeyConfig != null)
            {
                var keyControl = Keyboard.current[HotkeyConfig.Value];
                if (keyControl != null && keyControl.wasPressedThisFrame)
                {
                    if (WorldFlowManager.Visible && !GameUI.MenuVisible && !HDCaptureEngine.IsCapturing)
                    {
                        ToggleTool();
                    }
                }
            }
        }
    }
}
