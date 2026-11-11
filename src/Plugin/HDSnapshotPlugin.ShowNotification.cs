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
    }
}
