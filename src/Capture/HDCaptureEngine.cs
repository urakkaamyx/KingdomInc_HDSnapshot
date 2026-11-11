using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Behaviour.UI.Shared;
using Assets.Source.Player;
using HarmonyLib;
using UnityEngine;

namespace KingdomHDSnapshot
{
    public static partial class HDCaptureEngine
    {
        static readonly AccessTools.FieldRef<WorldFlowCamera, Camera> CameraField =
            AccessTools.FieldRefAccess<WorldFlowCamera, Camera>("_camera");
        static readonly AccessTools.FieldRef<WorldFlowCamera, SpriteRenderer> BackgroundField =
            AccessTools.FieldRefAccess<WorldFlowCamera, SpriteRenderer>("_background");

        public static bool IsCapturing { get; private set; }

        public static IEnumerator CaptureRoutine(
            SelectionBox bounds,
            float resolutionScale,
            BackgroundStyle backgroundStyle,
            int tileSize,
            bool copyToClipboard,
            bool autoOpenFolder,
            Action<string> onComplete,
            Action<string> onError)
        {
            if (IsCapturing)
            {
                onError?.Invoke("A snapshot is already in progress.");
                yield break;
            }

            var flowCam = WorldFlowCamera.Instance;
            if (!flowCam)
            {
                onError?.Invoke("WorldFlowCamera not found.");
                yield break;
            }

            Camera cam = CameraField(flowCam);
            if (!cam)
            {
                onError?.Invoke("Main workshop camera not found.");
                yield break;
            }

            if (!bounds.HasArea)
            {
                onError?.Invoke("Selected area is too small.");
                yield break;
            }

            IsCapturing = true;

            // Give Unity one frame to ensure any drag boxes/handles are hidden
            yield return null;

            // 1. Save original camera & environment state
            Vector3 originalCamPos = cam.transform.position;
            float originalOrthoSize = cam.orthographicSize;
            float originalAspect = cam.aspect;
            RenderTexture originalTarget = cam.targetTexture;
            Rect originalRect = cam.rect;
            CameraClearFlags originalClearFlags = cam.clearFlags;
            Color originalBgColor = cam.backgroundColor;
            int originalZoom = flowCam.ZoomLevel;

            SpriteRenderer background = BackgroundField(flowCam);
            bool originalBgEnabled = background != null && background.enabled;

            bool uiToggled = false;
            Texture2D finalTexture = null;
            string savedFilePath = null;

            try
            {
                // 2. Hide Screen-Space Overlay UI
                if (GameUI.Instance)
                {
                    GameUI.Instance.ToggleUI(false);
                    uiToggled = true;
                }

                // 3. Normalize Font Filtering & Deco Handles for 1:1 crisp render
                flowCam.SetZoomLevel(3);
                FontFilteringSwitch.UpdateCameraZoom(flowCam);
                WorldFlowDeco.UpdateZoomLevel(3);

                // 4. Configure background handling
                if (backgroundStyle == BackgroundStyle.Transparent)
                {
                    if (background) background.enabled = false;
                    cam.clearFlags = CameraClearFlags.SolidColor;
                    cam.backgroundColor = Color.clear;
                }
                else if (backgroundStyle == BackgroundStyle.DarkSlate)
                {
                    if (background) background.enabled = false;
                    cam.clearFlags = CameraClearFlags.SolidColor;
                    cam.backgroundColor = new Color(0.11f, 0.10f, 0.14f, 1f);
                }
                else
                {
                    // Native Workshop Grid
                    if (background) background.enabled = true;
                }

                // 5. Calculate Tiling & Output Dimensions
                int totalWorldW = bounds.Width;
                int totalWorldH = bounds.Height;

                resolutionScale = Mathf.Clamp(resolutionScale, 0.5f, 4f);
                int totalPixelW = Mathf.RoundToInt(totalWorldW * resolutionScale);
                int totalPixelH = Mathf.RoundToInt(totalWorldH * resolutionScale);

                // Enforce minimum and maximum texture bounds
                int maxTexSize = Math.Min(16384, SystemInfo.maxTextureSize);
                if (totalPixelW > maxTexSize || totalPixelH > maxTexSize)
                {
                    float ratio = Math.Min((float)maxTexSize / totalPixelW, (float)maxTexSize / totalPixelH);
                    totalPixelW = Mathf.RoundToInt(totalPixelW * ratio);
                    totalPixelH = Mathf.RoundToInt(totalPixelH * ratio);
                    resolutionScale *= ratio;
                }

                finalTexture = new Texture2D(totalPixelW, totalPixelH, TextureFormat.RGBA32, false);

                tileSize = Mathf.Clamp(tileSize, 512, 4096);
                int cols = Mathf.CeilToInt((float)totalWorldW / tileSize);
                int rows = Mathf.CeilToInt((float)totalWorldH / tileSize);

                // 6. Render tiles row by row, column by column
                for (int r = 0; r < rows; r++)
                {
                    int tileWorldY = bounds.Bottom + r * tileSize;
                    int tileWorldH = Math.Min(tileSize, bounds.Top - tileWorldY);
                    int tilePixelH = Mathf.RoundToInt(tileWorldH * resolutionScale);
                    int destPixelY = Mathf.RoundToInt(r * tileSize * resolutionScale);

                    for (int c = 0; c < cols; c++)
                    {
                        int tileWorldX = bounds.Left + c * tileSize;
                        int tileWorldW = Math.Min(tileSize, bounds.Right - tileWorldX);
                        int tilePixelW = Mathf.RoundToInt(tileWorldW * resolutionScale);
                        int destPixelX = Mathf.RoundToInt(c * tileSize * resolutionScale);

                        float tileCenterX = tileWorldX + tileWorldW / 2f;
                        float tileCenterY = tileWorldY + tileWorldH / 2f;

                        // Position camera precisely over tile
                        cam.transform.position = new Vector3(tileCenterX, tileCenterY, originalCamPos.z);
                        cam.orthographic = true;
                        cam.orthographicSize = tileWorldH / 2f;
                        cam.aspect = (float)tileWorldW / tileWorldH;

                        // Align background sprite renderer if active
                        if (background && background.enabled)
                        {
                            background.transform.position = new Vector3(tileCenterX, tileCenterY, background.transform.position.z);
                        }

                        // Create temporary RenderTexture for tile
                        RenderTexture rt = RenderTexture.GetTemporary(tilePixelW, tilePixelH, 24, RenderTextureFormat.ARGB32);
                        rt.filterMode = FilterMode.Point;
                        cam.targetTexture = rt;

                        cam.Render();

                        RenderTexture.active = rt;
                        finalTexture.ReadPixels(new Rect(0, 0, tilePixelW, tilePixelH), destPixelX, destPixelY);

                        cam.targetTexture = null;
                        RenderTexture.active = null;
                        RenderTexture.ReleaseTemporary(rt);

                        // Yield a frame periodically on huge jobs to keep UI responsive
                        if (cols * rows > 4 && (c + r * cols) % 2 == 0)
                        {
                            yield return null;
                        }
                    }
                }

                finalTexture.Apply();

                // 7. Save PNG to disk
                byte[] pngBytes = finalTexture.EncodeToPNG();

                string gameRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                string screenshotsDir = Path.Combine(gameRoot, "Screenshots");
                if (!Directory.Exists(screenshotsDir))
                {
                    Directory.CreateDirectory(screenshotsDir);
                }

                string flowName = "Workshop";
                try
                {
                    if (GamePlayer.Current != null && GamePlayer.Current.ActiveFlow != null)
                    {
                        flowName = $"Flow_{GamePlayer.Current.CurrentFlow + 1}";
                    }
                }
                catch { }

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"Snapshot_{flowName}_{timestamp}_{totalPixelW}x{totalPixelH}.png";
                savedFilePath = Path.Combine(screenshotsDir, fileName);

                File.WriteAllBytes(savedFilePath, pngBytes);

                // 8. Clipboard Copy
                if (copyToClipboard)
                {
                    ClipboardHelper.CopyImageAndPathToClipboard(finalTexture, savedFilePath);
                }

                // 9. Auto Open in Explorer
                if (autoOpenFolder && File.Exists(savedFilePath))
                {
                    try
                    {
                        Process.Start("explorer.exe", $"/select,\"{savedFilePath}\"");
                    }
                    catch { }
                }

                UISounds.TurnPage();
            }
            finally
            {
                // 10. Always restore original camera & UI state
                cam.targetTexture = originalTarget;
                cam.transform.position = originalCamPos;
                cam.orthographicSize = originalOrthoSize;
                cam.aspect = originalAspect;
                cam.rect = originalRect;
                cam.clearFlags = originalClearFlags;
                cam.backgroundColor = originalBgColor;
                cam.ResetAspect();

                if (background)
                {
                    background.enabled = originalBgEnabled;
                }

                flowCam.SetZoomLevel(originalZoom);
                FontFilteringSwitch.Cleanup();
                WorldFlowDeco.UpdateZoomLevel(originalZoom);

                if (uiToggled && GameUI.Instance)
                {
                    GameUI.Instance.ToggleUI(true);
                }

                if (finalTexture != null)
                {
                    UnityEngine.Object.Destroy(finalTexture);
                }

                IsCapturing = false;
            }

            if (!string.IsNullOrEmpty(savedFilePath))
            {
                onComplete?.Invoke(savedFilePath);
            }
        }
    }
}
