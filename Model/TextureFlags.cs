using System;

namespace OpenNova.Model;

[Flags]
public enum TextureFlags
{
    Animated = 1 << 0,
    Overrideable = 1 << 1,
    UnknownFlag = 1 << 2
}
