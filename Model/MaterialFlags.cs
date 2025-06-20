using System;

namespace OpenNova.Model;

[Flags]
public enum MaterialFlags
{
    AlphaTest = 1 << 0,
    AlphaTestInverted = 1 << 1,
    TwoSided = 1 << 2
}
