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
        public void SetToolActive(bool active)
        {
            if (buttonImage)
            {
                buttonImage.color = active ? new Color(0.4f, 0.85f, 1f, 1f) : Color.white;
            }
        }
    }
}
