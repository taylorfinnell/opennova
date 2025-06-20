using System;

namespace OpenNova.Definition
{
    [Flags]
    public enum EntityAttributes2 : uint
    {
        None = 0,
        VehicleBay = 0x1,
        AutoInheritTeam = 0x2,
        VehicleSpawn = 0x4,
        DynamicShadow = 0x10,
        StaticShadow = 0x20,
        TunnelPiece = 0x40,
        UseVK = 0x80,
        StaticDeath = 0x100,
        OnTurret = 0x400,
        HasTurret = 0x800,
        IsTurret = 0x1000,
        Farp = 0x2000,
        LandMine = 0x4000
    }
}