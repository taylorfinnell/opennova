using System;

namespace OpenNova.Mission
{
    [Flags]
    public enum MissionWaypointFlags : uint
    {
        None = 0,
        DoesNotLoop = 1 << 0,     // Bit 0 (value 1) - waypoint does not loop
        BlueTeam = 1 << 1,        // Bit 1 (value 2) - blue team waypoint
        RedTeam = 1 << 2          // Bit 2 (value 4) - red team waypoint
    }
}
