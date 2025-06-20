using Godot;
using OpenNova.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace OpenNova.Mission
{
    [GlobalClass]
    public partial class MissionHeader : Resource
    {
        public string Magic { get; set; }
        [Export] public string MissionName { get; set; }
        [Export] public string Designer { get; set; }
        [Export] public string Terrain { get; set; }
        [Export] public string DefaultStr { get; set; }
        [Export] public ClimateType Climate { get; set; }
        public byte[] Unknown0 { get; set; }
        [Export] public byte[] FogColor { get; set; }
        public uint NumItems { get; set; }
        public uint NumBuildings { get; set; }
        public uint NumMarkers { get; set; }
        public uint NumPeople { get; set; }
        public uint NumEvents { get; set; }
        public byte[] Unknown3 { get; set; }
        [Export] public string Environment { get; set; }
        public byte[] Unknown4 { get; set; }
        [Export] public byte[] WaterColor { get; set; }
        public uint Murk { get; set; }
        public byte[] Unknown5 { get; set; }
        [Export] public uint Music { get; set; }
        [Export] public uint Reverb { get; set; }
        [Export] public uint Health { get; set; }
        [Export] public uint Mana { get; set; }
        [Export] public string TerrainTile { get; set; }
        [Export] public string MissionBriefing { get; set; }
        [Export] public MissionType MissionType { get; set; }
        public byte[] Unknown7 { get; set; }
        public ushort WeaponLoadoutChunkLen { get; set; }
        [Export] public ushort BonusExpiration { get; set; }
        public ushort Unknown8 { get; set; }
        [Export] public ushort StartTime { get; set; }
        [Export] public ushort MinutesPerDay { get; set; }
        public byte[] Unknown9 { get; private set; }
        [Export] public AttribFlags AttribFlags { get; set; }
        [Export] public WeatherType WeatherType { get; set; }
        [Export] public byte MaxSaves { get; set; }
        [Export] public short AreaTriggerCount { get; internal set; }
        public ushort Something { get; internal set; }
        public byte Something1 { get; internal set; }
        [Export] public uint WindSpeed { get; internal set; }
        [Export] public float MapZoom { get; internal set; }
        [Export] public byte[] WinConditions { get; internal set; } // 8 bytes
        [Export] public byte[] LoseConditions { get; internal set; } // 8 bytes
        [Export] public uint WindDirection { get; internal set; }
        [Export] public ushort WaterOverride { get; internal set; }
        [Export] public ushort FogOverride { get; internal set; }
        public uint Unknown1 { get; internal set; }
        public byte Unknown2 { get; internal set; }
        public short Unknown6 { get; internal set; }

        public void Deserialize(ONBinaryReader reader)
        {
            reader.MarkPosition();

            Magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (!Magic.StartsWith("BMS"))
            {
                throw new Exception("Invalid BMS magic");
            }

            MissionName = reader.ReadFixedString(32);
            Designer = reader.ReadFixedString(32);
            Terrain = reader.ReadFixedString(48);
            DefaultStr = reader.ReadFixedString(16);
            Climate = (ClimateType)reader.ReadUInt32();
            AttribFlags = (AttribFlags)reader.ReadUInt32();

            Unknown0 = reader.ReadBytes(12);
            AssertAllBytesZero(Unknown0);

            WaterOverride = reader.ReadUInt16();
            Unknown1 = reader.ReadUInt32();
            FogOverride = reader.ReadUInt16();
            FogColor = reader.ReadBytes(3);

            Unknown2 = reader.ReadByte(); // Fog alpha??
            Debug.Assert(Unknown2 == 0, "Unknown was not 0");

            NumItems = reader.ReadUInt32();
            NumBuildings = reader.ReadUInt32();
            NumMarkers = reader.ReadUInt32();
            NumPeople = reader.ReadUInt32();
            NumEvents = reader.ReadUInt32();

            WeatherType = (WeatherType)reader.ReadUInt32();
            WinConditions = reader.ReadBytes(8);
            LoseConditions = reader.ReadBytes(8);

            Unknown3 = reader.ReadBytes(16);
            AssertAllBytesZero(Unknown3);

            Environment = reader.ReadFixedString(16);

            Unknown4 = reader.ReadBytes(10);
            AssertAllBytesZero(Unknown4);

            WaterColor = reader.ReadBytes(3);
            Murk = reader.ReadUInt16();
            Something1 = reader.ReadByte();
            WindSpeed = reader.ReadUInt32();
            WindDirection = reader.ReadUInt32();
            Unknown5 = reader.ReadBytes(4);

            Health = reader.ReadUInt32();
            Mana = reader.ReadUInt32();
            Music = reader.ReadUInt32();
            Reverb = reader.ReadUInt32();

            TerrainTile = reader.ReadFixedString(16);
            MissionBriefing = reader.ReadFixedString(256);

            Unknown6 = reader.ReadInt16();
            Debug.Assert(Unknown6 == 0);

            MissionType = (MissionType)reader.ReadByte();
            MaxSaves = reader.ReadByte();

            Unknown7 = reader.ReadBytes(16);
            AssertAllBytesZero(Unknown7);

            MapZoom = reader.ReadSingle();
            AreaTriggerCount = reader.ReadInt16();
            WeaponLoadoutChunkLen = reader.ReadUInt16();
            BonusExpiration = reader.ReadUInt16();
            Unknown8 = reader.ReadUInt16();
            Debug.Assert(Unknown8 == 0, "Unknown8 not 0");

            StartTime = reader.ReadUInt16();
            MinutesPerDay = reader.ReadUInt16();

            Unknown9 = reader.ReadBytes(28);
            AssertAllBytesZero(Unknown9);

            reader.AssertBytesRead(616);
        }

        public void Serialize(BinaryWriter writer)
        {
            // Magic
            writer.Write(Encoding.ASCII.GetBytes(Magic.PadRight(4, '\0')));

            // Strings
            writer.Write(Encoding.ASCII.GetBytes(MissionName.PadRight(32, '\0')));
            writer.Write(Encoding.ASCII.GetBytes(Designer.PadRight(32, '\0')));
            writer.Write(Encoding.ASCII.GetBytes(Terrain.PadRight(48, '\0')));
            writer.Write(Encoding.ASCII.GetBytes(DefaultStr.PadRight(16, '\0')));

            // Enums and flags
            writer.Write((uint)Climate);
            writer.Write((uint)AttribFlags);

            // Unknown data and arrays
            writer.Write(Unknown0 ?? new byte[12]);
            writer.Write(WaterOverride);
            writer.Write(Unknown1);
            writer.Write(FogOverride);
            writer.Write(FogColor ?? new byte[3]);
            writer.Write(Unknown2);

            // Entity counts
            writer.Write(NumItems);
            writer.Write(NumBuildings);
            writer.Write(NumMarkers);
            writer.Write(NumPeople);
            writer.Write(NumEvents);

            // Weather and conditions
            writer.Write((uint)WeatherType);
            writer.Write(WinConditions ?? new byte[8]);
            writer.Write(LoseConditions ?? new byte[8]);

            writer.Write(Unknown3 ?? new byte[16]);

            // Environment and water
            writer.Write(Encoding.ASCII.GetBytes(Environment.PadRight(16, '\0')));
            writer.Write(Unknown4 ?? new byte[10]);
            writer.Write(WaterColor ?? new byte[3]);
            writer.Write((ushort)Murk);
            writer.Write(Something1);
            writer.Write(WindSpeed);
            writer.Write(WindDirection);
            writer.Write(Unknown5 ?? new byte[4]);

            // Resources
            writer.Write(Health);
            writer.Write(Mana);
            writer.Write(Music);
            writer.Write(Reverb);

            // More strings
            writer.Write(Encoding.ASCII.GetBytes(TerrainTile.PadRight(16, '\0')));
            writer.Write(Encoding.ASCII.GetBytes(MissionBriefing.PadRight(256, '\0')));

            writer.Write(Unknown6);
            writer.Write((byte)MissionType);
            writer.Write(MaxSaves);
            writer.Write(Unknown7 ?? new byte[16]);

            // Final fields
            writer.Write(MapZoom);
            writer.Write(AreaTriggerCount);
            writer.Write(WeaponLoadoutChunkLen);
            writer.Write(BonusExpiration);
            writer.Write(Unknown8);
            writer.Write(StartTime);
            writer.Write(MinutesPerDay);
            writer.Write(Unknown9 ?? new byte[28]);
        }

        private static void AssertAllBytesZero(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                Debug.Assert(bytes[i] == 0, $"Byte at index {i} is not zero: {bytes[i]}");
            }
        }
    }
}
