using System;
using UnityEngine;

namespace KingdomHDSnapshot
{
    public readonly partial struct SelectionBox
    {
        public SelectionBox IncludePoint(int x, int y)
        {
            return new SelectionBox(Math.Min(Left, x), Math.Min(Bottom, y), Math.Max(Right, x), Math.Max(Top, y));
        }
    }
}
