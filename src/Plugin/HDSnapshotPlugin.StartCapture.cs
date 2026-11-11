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
    }
}
