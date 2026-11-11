using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace KingdomHDSnapshot
{
    public static partial class ClipboardHelper
    {
        const uint CF_DIB = 8;
        const uint GMEM_MOVEABLE = 0x0002;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool OpenClipboard(IntPtr hWndNewOwner);
    }
}
