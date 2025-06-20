using System;

namespace OpenNova.Model;

[Flags]
public enum CollisionFaceFlags : int
{
    None = 0,
    Bit0 = 0x0001,
    Bit1 = 0x0002,
    MoveIgnore = 0x0100,
    Scarring = 0x0400,
    BackfaceIgnore = 0x0800,
}
