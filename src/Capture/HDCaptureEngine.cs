using KingdomFoundation.Api;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace KingdomHDSnapshot
{
    public static class HDCaptureEngine
    {
        public static bool IsCapturing { get; private set; }

        public static IEnumerator CaptureRoutine(SelectionBox bounds, float resolutionScale, BackgroundStyle backgroundStyle, int tileSize, bool copyToClipboard, bool autoOpenFolder, Action<string> onComplete, Action<string> onError)
        {
            if (IsCapturing)
            {
                onError?.Invoke("A snapshot is already in progress.");
                yield break;
            }

            var flowCam = FoundationApi.CameraAccess.ActiveView;
            if (!flowCam)
            {
                onError?.Invoke("WorldFlowCamera not found.");
                yield break;
            }

            Camera cam = FoundationApi.CameraAccess.Camera(flowCam);
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
            int originalZoom = FoundationApi.CameraAccess.ZoomLevel(flowCam);
            SpriteRenderer background = FoundationApi.CameraAccess.Background(flowCam);
            bool originalBgEnabled = background != null && background.enabled;
            bool uiToggled = false;
            Texture2D finalTexture = null;
            string savedFilePath = null;
            try
            {
                // 2. Hide Screen-Space Overlay UI
                if (FoundationApi.UiAccess.Root)
                {
                    FoundationApi.UiAccess.SetUiVisible(false);
                    uiToggled = true;
                }

                // 3. Normalize Font Filtering & Deco Handles for 1:1 crisp render
                FoundationApi.CameraAccess.SetZoomLevel(flowCam, 3);
                FoundationApi.CameraAccess.RefreshFontFiltering(flowCam);
                FoundationApi.CameraAccess.UpdateDecorationZoom(3);
                // 4. Configure background handling
                if (backgroundStyle == BackgroundStyle.Transparent)
                {
                    if (background)
                        background.enabled = false;
                    cam.clearFlags = CameraClearFlags.SolidColor;
                    cam.backgroundColor = Color.clear;
                }
                else if (backgroundStyle == BackgroundStyle.DarkSlate)
                {
                    if (background)
                        background.enabled = false;
                    cam.clearFlags = CameraClearFlags.SolidColor;
                    cam.backgroundColor = new Color(0.11f, 0.10f, 0.14f, 1f);
                }
                else
                {
                    // Native Workshop Grid
                    if (background)
                        background.enabled = true;
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
                    if (FoundationApi.World.IsInKingdom)
                    {
                        flowName = $"Flow_{FoundationApi.World.ActiveFlowIndex + 1}";
                    }
                }
                catch
                {
                }

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
                    catch
                    {
                    }
                }

                FoundationApi.UiAccess.PlaySound(GameUiSound.TurnPage);
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

                FoundationApi.CameraAccess.SetZoomLevel(flowCam, originalZoom);
                FoundationApi.CameraAccess.CleanupFontFiltering();
                FoundationApi.CameraAccess.UpdateDecorationZoom(originalZoom);
                if (uiToggled && FoundationApi.UiAccess.Root)
                {
                    FoundationApi.UiAccess.SetUiVisible(true);
                }

                if (finalTexture != null)
                {
                    global::UnityEngine.Object.Destroy(finalTexture);
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
