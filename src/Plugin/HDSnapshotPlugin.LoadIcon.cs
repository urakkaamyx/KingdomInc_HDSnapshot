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
    }
}
