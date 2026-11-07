using System;
using UnityEngine;

namespace KingdomHDSnapshot
{
    public readonly struct SelectionBox
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

        public SelectionBox Expand(int margin)
        {
            return new SelectionBox(Left - margin, Bottom - margin, Right + margin, Top + margin);
        }

        public SelectionBox IncludePoint(int x, int y)
        {
            return new SelectionBox(Math.Min(Left, x), Math.Min(Bottom, y), Math.Max(Right, x), Math.Max(Top, y));
        }

        public SelectionBox IncludeFrame(int x, int y, int width, int height)
        {
            // In Kingdom Inc, position is top-left with positive width and downward height.
            int elemLeft = x;
            int elemRight = x + width;
            int elemTop = y;
            int elemBottom = y - height;

            return new SelectionBox(
                Math.Min(Left, elemLeft),
                Math.Min(Bottom, elemBottom),
                Math.Max(Right, elemRight),
                Math.Max(Top, elemTop)
            );
        }

        public bool Encloses(int x, int y, int width, int height)
        {
            return HasArea && width > 0 && height > 0
                && x >= Left && (long)x + width <= Right
                && y <= Top && (long)y - height >= Bottom;
        }

        public override string ToString() => $"SelectionBox({Left}, {Bottom}, {Right}, {Top} [{Width}x{Height}])";
    }
}

