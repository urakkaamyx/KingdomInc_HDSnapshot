using System;
using UnityEngine;

namespace KingdomHDSnapshot
{
    public readonly partial struct SelectionBox
    {
        public bool Encloses(int x, int y, int width, int height)
        {
            return HasArea && width > 0 && height > 0
                && x >= Left && (long)x + width <= Right
                && y <= Top && (long)y - height >= Bottom;
        }
    }
}
