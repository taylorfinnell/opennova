namespace OpenNova.Mission
{

    [System.Flags]
    public enum AttribFlags : uint
    {
        None = 0,
        WaterOverrideEnable = 0x1,          // bit 0
        FogDistanceOverrideEnable = 0x2,    // bit 1
        FogColorOverrideEnable = 0x4,       // bit 2
        WeatherOverrideEnable = 0x8,        // bit 3
        RotateMap180 = 0x20,                // bit 5
        SinglePlayerRespawn = 0x40,         // bit 6
        AdvanceAndSecure = 0x10000,         // bit 16 = map type 9
        ConquerAndControl = 0x20000,        // bit 17 = map type 10
        EnableNVG = 0x100000,               // bit 20
        StartWithNVGOn = 0x400000,          // bit 22
        AttackAndDefend = 0x800000,         // bit 23 = map type 6
        Coop = 0x1000000,                   // bit 24 = map type 2
        Deathmatch = 0x2000000,             // bit 25 = map type 11
        KingOfTheHill = 0x4000000,          // bit 26 = map type 4
        FlagBall = 0x8000000,               // bit 27 = map type 8
        CaptureTheFlag = 0x10000000,        // bit 28 = map type 7
        TeamDeathmatch = 0x20000000,        // bit 29 = map type 1
        TeamKingOfTheHill = 0x40000000,     // bit 30 = map type 3
        SearchAndDestroy = 0x80000000       // bit 31 = map type 5
    }
}
