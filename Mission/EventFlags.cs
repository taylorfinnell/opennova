using System;

namespace OpenNova.Mission
{
    [Flags]
    public enum EventFlags
    {
        None = 0,
        ResetAfter = 1,
        PreMission = 2,
        PostMission = 4,
        Unknown4 = 16,     // 0x10 - found in real mission files
        Unknown5 = 32      // 0x20 - found in real mission files
    }
}
