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

namespace KingdomHDSnapshot
{
    [BepInPlugin(Id, "HD Snapshot", Version)]
    [BepInDependency("urakka.kingdominc.foundation", BepInDependency.DependencyFlags.SoftDependency)]
    public sealed class HDSnapshotPlugin : BaseUnityPlugin
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
            FoundationIntegration.RegisterConfig(this);

            Logger.LogInfo($"HD Snapshot v{Version} loaded. Default hotkey: {HotkeyConfig.Value}.");
        }

        void LoadIcon()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                using (var stream = asm.GetManifestResourceStream("camera-icon.png") ?? asm.GetManifestResourceStream("HDSnapshot.Assets.camera-icon.png"))
                {
                    if (stream != null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                            tex.LoadImage(ms.ToArray());
                            tex.filterMode = FilterMode.Point;
                            cameraIcon = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Could not load embedded camera icon: {ex.Message}");
            }

            // Fallback: programmatic 16x16 camera icon
            var fallback = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color brass = new Color(0.8f, 0.65f, 0.35f, 1f);
            Color dark = new Color(0.15f, 0.12f, 0.1f, 1f);
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    if (y >= 4 && y <= 12 && x >= 2 && x <= 13)
                        fallback.SetPixel(x, y, (x >= 6 && x <= 9 && y >= 6 && y <= 10) ? dark : brass);
                    else
                        fallback.SetPixel(x, y, Color.clear);
                }
            }
            fallback.Apply();
            fallback.filterMode = FilterMode.Point;
            cameraIcon = Sprite.Create(fallback, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 100);
        }

        void Update()
        {
            var ui = GameUI.Instance;
            if (!ui || GamePlayer.Current == null)
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

        static T GetPrivateField<T>(object source, string name) where T : class
        {
            return typeof(GameUI).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(source) as T;
        }

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

        public void ToggleTool()
        {
            if (areaSelector == null) return;

            if (areaSelector.Active)
            {
                areaSelector.Cancel();
            }
            else
            {
                // Ensure native tools don't conflict
                if (GameUI.Instance)
                {
                    GameUI.Instance.ToggleCloneMode(false);
                    GameUI.Instance.ToggleAreaMove(false);
                }
                areaSelector.Begin();
            }
        }

        public void SetToolActive(bool active)
        {
            if (buttonImage)
            {
                buttonImage.color = active ? new Color(0.4f, 0.85f, 1f, 1f) : Color.white;
            }
        }

        public void StartCapture(SelectionBox bounds)
        {
            StartCoroutine(HDCaptureEngine.CaptureRoutine(
                bounds,
                ResolutionScale,
                BgStyleConfig.Value,
                TileSizeConfig.Value,
                CopyToClipboardConfig.Value,
                AutoOpenFolderConfig.Value,
                onComplete: path =>
                {
                    string fileName = Path.GetFileName(path);
                    ShowNotification($"HD Snapshot saved: {fileName} (copied to clipboard)");
                },
                onError: err =>
                {
                    ShowNotification($"Snapshot error: {err}");
                }
            ));
        }

        public void ShowNotification(string message)
        {
            Logger.LogInfo(message);
            try
            {
                if (WorldFlowManager.Visible && Camera.main)
                {
                    Vector2 pos = WorldFlowCamera.Instance ? (Vector2)WorldFlowCamera.Instance.transform.position : Vector2.zero;
                    WorldFlowManager.Instance.ShowFloatingIconText(pos, cameraIcon, message);
                }
            }
            catch { }
        }

        void CleanupUi()
        {
            if (toolbarButton)
            {
                Destroy(toolbarButton);
                toolbarButton = null;
            }
            if (areaSelector)
            {
                Destroy(areaSelector.gameObject);
                areaSelector = null;
            }
            attachedUi = null;
        }

        void OnDestroy()
        {
            CleanupUi();
        }
    }
}
