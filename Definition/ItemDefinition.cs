using Godot;
using OpenNova.Definition;

[Tool]
[GlobalClass]
public partial class ItemDefinition : Resource
{
    [Export] public string Name { get; set; } = "";
    [Export] public int Id { get; set; }
    [Export] public int InternalId { get; set; } // Used in game (Id - 100000)
    [Export] public EntityType EntityType { get; set; }
    [Export] public string SubType { get; set; } = "";
    [Export] public PackedScene Graphic { get; set; }
    [Export] public string GraphicEnemy { get; set; } = "";
    [Export] public float Scale { get; set; } = 1.0f;
    [Export] public float Hp { get; set; }
    [Export] public float Armor { get; set; }
    [Export] public float CriticalHp { get; set; }
    [Export] public float CriticalDrain { get; set; }
    [Export] public float RadarSig { get; set; }
    [Export] public float HeatSig { get; set; }
    [Export] public float Score { get; set; }
    [Export] public int DeathTime { get; set; }
    [Export] public float DamageReduction { get; set; }
    [Export] public float DamageReductionScale { get; set; }
    [Export] public float Kz { get; set; }
    [Export] public EntityAttributes Attributes { get; set; }
    [Export] public EntityAttributes2 Attributes2 { get; set; }
    [Export] public bool Good { get; set; }
    [Export] public bool Evil { get; set; }
    [Export] public string DefaultAip { get; set; } = "";
    [Export] public string TextId { get; set; } = "";
    [Export] public byte UnitType { get; set; }
    [Export] public int SID { get; set; }
    [Export] public string PowerUpDef { get; set; } = "";
    [Export] public int PhraseSet { get; set; }
    [Export] public int ClipSize { get; set; }
    [Export] public string AmmoCloseAttack { get; set; } = "";
    [Export] public string AmmoEasyRocket { get; set; } = "";
    [Export] public string AmmoAdvancedRocket { get; set; } = "";
    [Export] public string AmmoMarker3 { get; set; } = "";
    [Export] public string LaunchUpsCloseAttack { get; set; } = "";
    [Export] public string LaunchUpsRocket { get; set; } = "";
    [Export] public string LaunchUpsMarker3 { get; set; } = "";
    [Export] public string PrimaryWeapon { get; set; } = "";
    [Export] public string Husk { get; set; } = "";
    [Export] public string HuskFinal { get; set; } = "";
    [Export] public float HuskSwapAtSec { get; set; }
    [Export] public float HuskSwapAt { get; set; }
    [Export] public byte HuskSubParts { get; set; }
    [Export] public Godot.Collections.Dictionary<int, byte> HuskSubPartTypes { get; set; }
    [Export] public string ShadowTexture { get; set; } = "";
    [Export] public float ShadowWidth { get; set; }
    [Export] public float ShadowHeight { get; set; }
    [Export] public float ShadowOffsetX { get; set; }
    [Export] public float ShadowOffsetY { get; set; }
    [Export] public string HuskShadow { get; set; } = "";
    [Export] public string AnimDef { get; set; } = "";
    [Export] public string VirtualDisplay { get; set; } = "";
    [Export] public string HudImage { get; set; } = "";
    [Export] public string AiFunction { get; set; } = "";
    [Export] public string RenderFunction { get; set; } = "";
    [Export] public string MoveFunction { get; set; } = "";
    [Export] public string DiskFunction { get; set; } = "";
    [Export] public string InputFunction { get; set; } = "";
    [Export] public string SoundProfile { get; set; } = "";
    [Export] public string SoundProfileFemale { get; set; } = "";
    [Export] public string SoundDeath { get; set; } = "";
    [Export] public Godot.Collections.Array<string> SoundLoops { get; set; }
    [Export] public string SoundLoop1 { get; set; } = "";
    [Export] public string SoundLoop2 { get; set; } = "";
    [Export] public string SoundLoop3 { get; set; } = "";
    [Export] public string SoundLoop4 { get; set; } = "";
    [Export] public string DawnShot { get; set; } = "";
    [Export] public int DawnShotMinTime { get; set; }
    [Export] public int DawnShotMaxTime { get; set; }
    [Export] public string DayShot { get; set; } = "";
    [Export] public int DayShotMinTime { get; set; }
    [Export] public int DayShotMaxTime { get; set; }
    [Export] public string DuskShot { get; set; } = "";
    [Export] public int DuskShotMinTime { get; set; }
    [Export] public int DuskShotMaxTime { get; set; }
    [Export] public string NightShot { get; set; } = "";
    [Export] public int NightShotMinTime { get; set; }
    [Export] public int NightShotMaxTime { get; set; }
    [Export] public string ParticleFx { get; set; } = "";
    [Export] public string ParticleFxSlot { get; set; } = "";
    [Export] public string ParticleFxS { get; set; } = "";
    [Export] public string ParticleFxSSlot { get; set; } = "";
    [Export] public string ParticleFxW1 { get; set; } = "";
    [Export] public string ParticleFxW1Slot { get; set; } = "";
    [Export] public string ParticleFxW2 { get; set; } = "";
    [Export] public string ParticleFxW2Slot { get; set; } = "";
    [Export] public string ParticleFxW3 { get; set; } = "";
    [Export] public string ParticleFxW3Slot { get; set; } = "";
    [Export] public string ParticleFxW4 { get; set; } = "";
    [Export] public string ParticleFxW4Slot { get; set; } = "";
    [Export] public string ParticleDeath { get; set; } = "";
    [Export] public string ParticleH2ODeath { get; set; } = "";
    [Export] public string ParticleFire { get; set; } = "";
    [Export] public string ParticleOther { get; set; } = "";
    [Export] public string ParticleFinale { get; set; } = "";
    [Export] public string ParticleSpawn { get; set; } = "";
    [Export] public EmplacedWeapon AddEWeap { get; set; }
    [Export] public EmplacedWeapon AddEWeapC { get; set; }
    [Export] public EmplacedWeapon AddEWeapG { get; set; }
    [Export] public byte NumDoors { get; set; }
    [Export] public byte FirstDoor { get; set; }
    [Export] public uint DoorType { get; set; }
    [Export] public uint DoorDir { get; set; }
    [Export] public int OpenRate { get; set; }
    [Export] public int MaxAngle { get; set; }
    [Export] public string DoorOpenSoundId { get; set; } = "";
    [Export] public string DoorCloseSoundId { get; set; } = "";

    public void Init()
    {
        HuskSubPartTypes = new Godot.Collections.Dictionary<int, byte>();
        SoundLoops = new Godot.Collections.Array<string>();
    }
}
