using System;
using UnityEngine;

namespace KingdomHDSnapshot
{
    public readonly partial struct SelectionBox
    {
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
    }
}
