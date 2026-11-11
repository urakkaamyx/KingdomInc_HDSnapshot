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
        static T GetPrivateField<T>(object source, string name) where T : class
        {
            return typeof(GameUI).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(source) as T;
        }
    }
}
