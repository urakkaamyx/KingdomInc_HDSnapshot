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
    }
}
