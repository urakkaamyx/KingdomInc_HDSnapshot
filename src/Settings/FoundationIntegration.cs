using System;
using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomHDSnapshot
{
    internal static partial class FoundationIntegration
    {
        public static void RegisterConfig(HDSnapshotPlugin plugin, FoundationModContext context)
        {
            var section = context.Settings("HD Snapshot",
                "High-definition box selection and panoramic screenshot tool for workshop layouts.");

            // Resolution Scale Dropdown
            string[] resOptions = { "1.0x (Native 1:1 HD)", "1.5x (Super HD)", "2.0x (Ultra HD)" };
            int currentResIdx = plugin.ResolutionScale >= 2f ? 2 : (plugin.ResolutionScale >= 1.5f ? 1 : 0);
            section.AddDropdown(
                "Resolution Scale",
                resOptions,
                currentResIdx,
                idx =>
                {
                    float scale = idx == 2 ? 2f : (idx == 1 ? 1.5f : 1f);
                    plugin.ResolutionScaleConfig.Value = scale;
                    plugin.Config.Save();
                },
                getter: () => plugin.ResolutionScale >= 2f ? 2 : (plugin.ResolutionScale >= 1.5f ? 1 : 0)
            );

            // Background Style Dropdown
            string[] bgOptions = { "Workshop Grid (Default)", "Transparent PNG", "Dark Slate" };
            section.AddDropdown(
                "Background Style",
                bgOptions,
                (int)plugin.BgStyleConfig.Value,
                idx =>
                {
                    plugin.BgStyleConfig.Value = (BackgroundStyle)idx;
                    plugin.Config.Save();
                },
                getter: () => (int)plugin.BgStyleConfig.Value
            );

            // Copy to Clipboard
            section.AddToggle(
                "Copy to Clipboard",
                plugin.CopyToClipboardConfig.Value,
                val =>
                {
                    plugin.CopyToClipboardConfig.Value = val;
                    plugin.Config.Save();
                },
                getter: () => plugin.CopyToClipboardConfig.Value
            );

            // Auto Open Folder
            section.AddToggle(
                "Open Folder After Capture",
                plugin.AutoOpenFolderConfig.Value,
                val =>
                {
                    plugin.AutoOpenFolderConfig.Value = val;
                    plugin.Config.Save();
                },
                getter: () => plugin.AutoOpenFolderConfig.Value
            );
        }
    }
}
