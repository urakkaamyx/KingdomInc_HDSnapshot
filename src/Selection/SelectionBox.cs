using System;
using UnityEngine;

namespace KingdomHDSnapshot
{
    public readonly partial struct SelectionBox
    {
        public readonly int Left;
        public readonly int Bottom;
        public readonly int Right;
        public readonly int Top;

        public int Width => Math.Max(0, Right - Left);
        public int Height => Math.Max(0, Top - Bottom);
        public bool HasArea => Width >= 16 && Height >= 16;
        public Vector2 Center => new Vector2(Left + Width / 2f, Bottom + Height / 2f);

        public SelectionBox(int x1, int y1, int x2, int y2)
        {
            Left = Math.Min(x1, x2);
            Right = Math.Max(x1, x2);
            Bottom = Math.Min(y1, y2);
            Top = Math.Max(y1, y2);
        }
    }
}
