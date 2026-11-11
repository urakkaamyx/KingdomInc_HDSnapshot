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
        [BepInPlugin(Id, "HD Snapshot", Version)]
        [BepInDependency("urakka.kingdominc.foundation", "0.11.0")]
    public sealed partial class HDSnapshotPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.hdsnapshot";
        public const string Version = "1.0.0";

        public static HDSnapshotPlugin Instance { get; private set; }
        public BepInEx.Logging.ManualLogSource Log => Logger;

        public ConfigEntry<float> ResolutionScaleConfig;
        public ConfigEntry<BackgroundStyle> BgStyleConfig;
        public ConfigEntry<int> TileSizeConfig;
        public ConfigEntry<bool> CopyToClipboardConfig;
        public ConfigEntry<bool> AutoOpenFolderConfig;
        public ConfigEntry<Key> HotkeyConfig;

        public float ResolutionScale => ResolutionScaleConfig?.Value ?? 1f;

        FoundationModContext foundation;
        GameUI attachedUi;
        GameObject toolbarButton;
        Image buttonImage;
        AreaSelector areaSelector;
        Sprite cameraIcon;
        float nextAttachTime;

        void Awake()
        {
            Instance = this;

            ResolutionScaleConfig = Config.Bind(
                "Capture", "ResolutionScale", 1.0f,
                new ConfigDescription("Render resolution scale. 1.0 = native pixel-perfect 1:1 HD; 2.0 = Ultra-HD supersampling.", new AcceptableValueRange<float>(0.5f, 4.0f))
            );

            BgStyleConfig = Config.Bind(
                "Capture", "BackgroundStyle", BackgroundStyle.WorkshopGrid,
                new ConfigDescription("Background style for saved snapshots.")
            );

            TileSizeConfig = Config.Bind(
                "Capture", "TileSize", 2048,
                new ConfigDescription("Tile chunk size in world units for rendering.", new AcceptableValueRange<int>(512, 4096))
            );

            CopyToClipboardConfig = Config.Bind(
                "Output", "CopyToClipboard", true,
                new ConfigDescription("Copy snapshot image directly to clipboard for pasting into Discord, image editors, etc.")
            );

            AutoOpenFolderConfig = Config.Bind(
                "Output", "AutoOpenFolder", true,
                new ConfigDescription("Automatically reveal saved image in Windows Explorer.")
            );

            HotkeyConfig = Config.Bind(
                "Input", "SnapshotHotkey", Key.F9,
                new ConfigDescription("Hotkey to activate HD Snapshot box select tool.")
            );

            LoadIcon();
            foundation = FoundationApi.ForMod(Id);
            FoundationIntegration.RegisterConfig(this, foundation);

            Logger.LogInfo($"HD Snapshot v{Version} loaded. Default hotkey: {HotkeyConfig.Value}.");
        }
    }
}
