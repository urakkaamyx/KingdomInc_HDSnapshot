using System;
using UnityEngine;

namespace KingdomHDSnapshot
{
    public readonly partial struct SelectionBox
    {
        public override string ToString() => $"SelectionBox({Left}, {Bottom}, {Right}, {Top} [{Width}x{Height}])";
    }
}
