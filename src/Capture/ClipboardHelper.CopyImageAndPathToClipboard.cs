using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace KingdomHDSnapshot
{
    public static partial class ClipboardHelper
    {
        public static bool CopyImageAndPathToClipboard(Texture2D texture, string filePath)
        {
            // 1. Set file path in Unity system copy buffer so text paste works
            try
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    GUIUtility.systemCopyBuffer = filePath;
                }
            }
            catch { }

            // 2. Place device-independent bitmap (CF_DIB) on Windows clipboard
            if (texture == null) return false;

            try
            {
                int width = texture.width;
                int height = texture.height;
                Color32[] pixels = texture.GetPixels32();

                // BITMAPINFOHEADER (40 bytes) + raw BGR (or BGRA) bytes
                // Row stride must be aligned to 4 bytes:
                int rowBytes = width * 4;
                int imageBytes = rowBytes * height;
                int totalBytes = 40 + imageBytes;

                IntPtr hGlobal = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)totalBytes);
                if (hGlobal == IntPtr.Zero) return false;

                IntPtr pGlobal = GlobalLock(hGlobal);
                if (pGlobal == IntPtr.Zero) return false;

                try
                {
                    // Write BITMAPINFOHEADER
                    Marshal.WriteInt32(pGlobal, 0, 40);             // biSize
                    Marshal.WriteInt32(pGlobal, 4, width);          // biWidth
                    Marshal.WriteInt32(pGlobal, 8, height);         // biHeight (positive = bottom-up)
                    Marshal.WriteInt16(pGlobal, 12, 1);             // biPlanes
                    Marshal.WriteInt16(pGlobal, 14, 32);            // biBitCount
                    Marshal.WriteInt32(pGlobal, 16, 0);             // biCompression (BI_RGB)
                    Marshal.WriteInt32(pGlobal, 20, imageBytes);     // biSizeImage
                    Marshal.WriteInt32(pGlobal, 24, 0);             // biXPelsPerMeter
                    Marshal.WriteInt32(pGlobal, 28, 0);             // biYPelsPerMeter
                    Marshal.WriteInt32(pGlobal, 32, 0);             // biClrUsed
                    Marshal.WriteInt32(pGlobal, 36, 0);             // biClrImportant

                    // Write pixel bytes (bottom-up, BGRA order)
                    // Texture2D.GetPixels32 is already bottom-up!
                    byte[] raw = new byte[imageBytes];
                    int byteIdx = 0;
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        Color32 c = pixels[i];
                        raw[byteIdx] = c.b;
                        raw[byteIdx + 1] = c.g;
                        raw[byteIdx + 2] = c.r;
                        raw[byteIdx + 3] = c.a;
                        byteIdx += 4;
                    }

                    Marshal.Copy(raw, 0, IntPtr.Add(pGlobal, 40), imageBytes);
                }
                finally
                {
                    GlobalUnlock(hGlobal);
                }

                if (!OpenClipboard(IntPtr.Zero)) return false;
                try
                {
                    EmptyClipboard();
                    SetClipboardData(CF_DIB, hGlobal);
                    return true;
                }
                finally
                {
                    CloseClipboard();
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
