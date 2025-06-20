using System;

namespace OpenNova.Definition
{
    [Flags]
    public enum EntityAttributes : uint
    {
        None = 0,
        MoveCb = 0x1,
        Powerup = 0x2,
        NoMoveShoot = 0x4,
        NoTool = 0x8,
        Snap = 0x10,
        EWeap = 0x20,
        PlayerControl = 0x40,
        Door = 0x80,
        NoTarget = 0x100,
        Landable = 0x200,
        Missile = 0x400,
        Takeable = 0x2000,
        FastRope = 0x1000,
        Easy = 0x4000,
        Train = 0x8000,
        FourTeam = 0x10000,
        ChangeTeam = 0x20000,
        SpawnPoint = 0x40000,
        Armory = 0x80000,
        AIData = 0x100000,
        LeaveCorpse = 0x400000,
        NoDismember = 0x800000,
        NoWeapon = 0x1000000,
        Reflect = 0x2000000,
        NoShadow = 0x4000000,
        Concave = 0x8000000,
        NoScar = 0x10000000,
        NoHud = 0x20000000,
        NoDie = 0x40000000
    }
}