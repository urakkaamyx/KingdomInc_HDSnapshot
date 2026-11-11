using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace KingdomHDSnapshot
{
    public static partial class ClipboardHelper
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool CloseClipboard();
    }
}
