using System;
using UnityEngine;

namespace KingdomHDSnapshot
{
    public readonly partial struct SelectionBox
    {
        public SelectionBox Expand(int margin)
        {
            return new SelectionBox(Left - margin, Bottom - margin, Right + margin, Top + margin);
        }
    }
}
