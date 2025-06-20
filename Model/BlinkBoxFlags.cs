using System;

namespace OpenNova.Model;

[Flags]
public enum BlinkBoxFlags
{
    U1 = 1 << 0,
    V = 1 << 1,
    S = 1 << 2,
    W = 1 << 3,
    L = 1 << 4,
    O = 1 << 5,

    U4 = 1 << 6, // Never seen this bit as 1
    U5 = 1 << 7  // Never seen this bit as 1
}
