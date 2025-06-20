using Godot;
using OpenNova.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace OpenNova.Mission
{
    [Flags]
    public enum BmsiAttributeFlags : uint
    {
        None = 0,
        Blind = 1 << 0,                 // Bit 0 (value 1)
        Guarding = 1 << 1,              // Bit 1 (value 2)
        RemoveIfLessThan = 1 << 4,      // Bit 4 (value 16) - when nolessthan > 0
        RemoveIfMoreThan = 1 << 5,      // Bit 5 (value 32) - when nomorethan > 0
        Multiplayer = 1 << 6,           // Bit 6 (value 64)
        Berserk = 1 << 11,              // Bit 11 (value 2048)
        FlyingOrganic = 1 << 14,        // Bit 14 (value 16384)
        Coward = 1 << 16,               // Bit 16 (value 65536)
        AdvancedAmmo = 1 << 18,         // Bit 18 (value 262144)
        Indestructible = 1 << 21,       // Bit 21 (value 2097152)
        NavigationWaypoint = 1 << 22,   // Bit 22 (value 4194304)
        Reflective = 1 << 23,           // Bit 23 (value 8388608)
        NoShadow = 1 << 24              // Bit 24 (value 16777216)
    }

    [GlobalClass]
    [Tool]
    public partial class MissionEntity : Resource
    {
        [Export] public ItemType Type { get; set; }
        [Export] public int TypeId { get; set; }        // 0x00 - type_id
        public short Unk { get; set; }           // 0x02
        [Export] public int NameIndex { get; set; }     // 0x04 - name_index
        public short Unk2 { get; set; }          // 0x06
        [Export] public int Id { get; set; }            // 0x08 - id
        public short Unk3 { get; set; }          // 0x0A
        [Export] public uint BmsiAttributes { get; set; } // 0x0C - AI attributes bitfield
        [Export] public double X { get; set; }             // 0x10
        [Export] public double Y { get; set; }             // 0x14
        [Export] public double Z { get; set; }             // 0x18
        [Export] public int WpDistance { get; set; }       // 0x1C - includes padding
        [Export] public int Perception2 { get; set; }      // 0x20 - perception2 (includes padding)
        [Export] public int Perfectionist2 { get; set; }   // 0x24 - perfectionist2 (includes padding)
        [Export] public int MinEngagementDistance { get; set; } // 0x28 - min engagement distance (includes padding)
        [Export] public int MaxEngagementDistance { get; set; } // 0x2C - max engagement distance (includes padding)
        [Export] public int WpNumber { get; set; }         // 0x30 - waypoint number (includes padding)
        [Export] public short WAccuracy2 { get; set; }     // 0x34 - weapon accuracy 2
        [Export] public short WAccuracy1 { get; set; }     // 0x36 - weapon accuracy 1
        [Export] public short Yaw { get; set; }            // 0x38 - rotation yaw
        [Export] public short Pitch { get; set; }          // 0x3A - rotation pitch  
        [Export] public short Roll { get; set; }           // 0x3C - rotation roll
        [Export] public short Spawns { get; set; }         // 0x3E - spawns count
        [Export] public byte CrouchTimer { get; set; }      // 0x40 - crouchtimer
        public byte Unk15a { get; set; }           // 0x41
        [Export] public short ShootTimer { get; set; }     // 0x42 - shoottimer
        [Export] public short WpAdvTrigger { get; set; }   // 0x44 - wp_adv_trigger
        [Export] public short Attention { get; set; }      // 0x46 - attention
        [Export] public byte AlertState { get; set; }      // 0x48 - alert_state
        [Export] public byte Team { get; set; }             // 0x49 - team
        [Export] public byte NoMoreThan { get; set; }       // 0x4A - nomorethan  
        [Export] public byte NoLessThan { get; set; }      // 0x4B - nolessthan
        public short Unk19 { get; set; }          // 0x4C
        [Export] public byte GroupId { get; set; }         // 0x4E - group_id
        [Export] public byte WaypointId { get; set; }       // 0x4F - waypoint_id
        [Export] public byte Obliqueness { get; set; }      // 0x50 - obliqueness
        [Export] public byte MapSymbol { get; set; }        // 0x51 - map_symbol
        public short Unk22 { get; set; }          // 0x52
        public short Unk23 { get; set; }          // 0x54
        public short Unk24 { get; set; }          // 0x56
        public short Unk25 { get; set; }          // 0x58
        public short Unk26 { get; set; }          // 0x5A
        [Export] public int FireTimer { get; set; }      // 0x5C - fire timer
        public short Unk27 { get; set; }          // 0x5E
        [Export] public int TToolIndex { get; set; }     // 0x60 - ttoolindex (may also be WP name index)
        public int Unk30_31 { get; set; }         // 0x64 - combined unknown field
        public int Unk36 { get; set; }            // 0x8C
        public int Unk37 { get; set; }            // 0x90
        public int Unk38 { get; set; }            // 0x94
        public int Unk39 { get; set; }            // 0x98
        [Export] public int MaxAttackDistance { get; set; } // 0x9C - max attack distance
        public int Unk41 { get; set; }            // 0xA0
        [Export] public byte ColorOverride { get; set; }    // 0xA4 - color_override  
        [Export] public byte TeamBudget { get; set; }       // 0xA5 - team_budget
        public short Unk42b { get; set; }          // 0xA6
        public int Unk43 { get; set; }            // 0xA8
        [Export] public string Name1 { get; set; }
        [Export] public string Name2 { get; set; }
        [Export] public string GenString { get; set; }

        public Vector3 Rotation => new Vector3(Pitch, Yaw, Roll);
        public Vector3 Position => new Vector3((float)X, (float)Y, (float)Z);

        public Vector3 GetWorldPosition()
        {
            var correctionMatrix = Transform3D.Identity.Rotated(Vector3.Right, -Mathf.Pi / 2);
            return correctionMatrix * Position;
        }

        public Quaternion GetWorldRotation()
        {
            float yawRad = -(Rotation.Y * Mathf.Pi / 180f) + Mathf.Pi;
            float pitchRad = -(Rotation.X * Mathf.Pi / 180f);
            float rollRad = (Rotation.Z * Mathf.Pi / 180f);
            return Quaternion.FromEuler(new Vector3(pitchRad, yawRad, rollRad));
        }

        public Transform3D GetWorldTransform()
        {
            var transform = Transform3D.Identity;
            transform.Origin = GetWorldPosition();
            transform.Basis = new Basis(GetWorldRotation());
            return transform;
        }

        public void Deserialize(ONBinaryReader reader)
        {
            reader.MarkPosition();

            TypeId = reader.ReadInt32();         // 0x00
            NameIndex = reader.ReadInt32();      // 0x04 - name_index
            Id = reader.ReadInt32();             // 0x08
            BmsiAttributes = reader.ReadUInt32(); // 0x0C
            X = reader.ReadInt32() / 65536f;
            Y = reader.ReadInt32() / 65536f;
            Z = reader.ReadInt32() / 65536f;
            WpDistance = reader.ReadInt32();     // 0x1C - includes padding
            Perception2 = reader.ReadInt32();    // 0x20 - includes padding
            Perfectionist2 = reader.ReadInt32(); // 0x24 - includes padding
            MinEngagementDistance = reader.ReadInt32(); // 0x28 - includes padding
            MaxEngagementDistance = reader.ReadInt32(); // 0x2C - includes padding
            WpNumber = reader.ReadInt32();       // 0x30 - includes padding
            WAccuracy2 = reader.ReadInt16();     // 0x34 - weapon accuracy 2
            WAccuracy1 = reader.ReadInt16();     // 0x36 - weapon accuracy 1
            Yaw = reader.ReadInt16();            // 0x38 - rotation yaw
            Pitch = reader.ReadInt16();          // 0x3A - rotation pitch
            Roll = reader.ReadInt16();           // 0x3C - rotation roll
            Spawns = reader.ReadInt16();         // 0x3E - spawns count
            CrouchTimer = reader.ReadByte();     // 0x40 - crouchtimer

            Unk15a = reader.ReadByte();          // 0x41
            Debug.Assert(Unk15a == 0);

            ShootTimer = reader.ReadInt16();     // 0x42 - shoottimer
            WpAdvTrigger = reader.ReadInt16();   // 0x44 - wp_adv_trigger
            Attention = reader.ReadInt16();      // 0x46 - attention
            AlertState = reader.ReadByte();      // 0x48 - alert_state
            Team = reader.ReadByte();             // 0x49 - team
            NoMoreThan = reader.ReadByte();       // 0x4A - nomorethan
            NoLessThan = reader.ReadByte();      // 0x4B - nolessthan

            Unk19 = reader.ReadInt16();          // 0x4C
            Debug.Assert(Unk19 == 0, $"Unk19 should be 0 but was {Unk19}");

            GroupId = reader.ReadByte();         // 0x4E - group_id
            WaypointId = reader.ReadByte();       // 0x4F - waypoint_id
            Obliqueness = reader.ReadByte();      // 0x50 - obliqueness
            MapSymbol = reader.ReadByte();        // 0x51 - map_symbol

            Unk22 = reader.ReadInt16();          // 0x52
            Debug.Assert(Unk22 == 0, $"Unk22 should be 0 but was {Unk22}");
            Unk23 = reader.ReadInt16();          // 0x54
            Debug.Assert(Unk23 == 0, $"Unk23 should be 0 but was {Unk23}");
            Unk24 = reader.ReadInt16();          // 0x56
            Debug.Assert(Unk24 == 0, $"Unk24 should be 0 but was {Unk24}");
            Unk25 = reader.ReadInt16();          // 0x58
            Debug.Assert(Unk25 == 0, $"Unk25 should be 0 but was {Unk25}");
            Unk26 = reader.ReadInt16();          // 0x5A
            Debug.Assert(Unk26 == 0, $"Unk26 should be 0 but was {Unk26}");

            FireTimer = reader.ReadInt32();      // 0x5C - fire timer
            TToolIndex = reader.ReadInt32();     // 0x60 - ttoolindex
            if (TToolIndex != -1 && TToolIndex != 0)
                GD.Print("TToolIndex: " + TToolIndex);

            Unk30_31 = reader.ReadInt32();       // 0x64 - combined unknown field
            Debug.Assert(Unk30_31 == 0, $"Unk30_31 should be 0 but was {Unk30_31}");

            Name1 = reader.ReadFixedString(8);   // 0x68 - iai_name
            Name2 = reader.ReadFixedString(8);   // 0x70 - ai_textfile
            GenString = reader.ReadFixedString(36); // 0x78 - gen_string
            MaxAttackDistance = reader.ReadInt32(); // 0x9C - max attack distance

            Unk41 = reader.ReadInt32();          // 0xA0
            Debug.Assert(Unk41 == 0, $"Unk41 should be 0 but was {Unk41}");

            ColorOverride = reader.ReadByte();   // 0xA4 - color_override
            TeamBudget = reader.ReadByte();      // 0xA5 - team_budget  

            Unk42b = reader.ReadInt16();         // 0xA6
            Debug.Assert(Unk42b == 0);
            Unk43 = reader.ReadInt32();          // 0xA8
            Debug.Assert(Unk43 == 0, $"Unk43 should be 0 but was {Unk43}");

            reader.AssertBytesRead(0xac);
        }

        public BmsiAttributeFlags GetAIFlags()
        {
            return (BmsiAttributeFlags)BmsiAttributes;
        }
        public void Serialize(BinaryWriter writer)
        {
            writer.Write(TypeId);                               // 0x00
            writer.Write(NameIndex);                            // 0x04
            writer.Write(Id);                                   // 0x08
            writer.Write(BmsiAttributes);                       // 0x0C
            writer.Write((int)(X * 65536f));                   // 0x10
            writer.Write((int)(Y * 65536f));                   // 0x14
            writer.Write((int)(Z * 65536f));                   // 0x18
            writer.Write(WpDistance);                           // 0x1C
            writer.Write(Perception2);                          // 0x20
            writer.Write(Perfectionist2);                       // 0x24
            writer.Write(MinEngagementDistance);                // 0x28
            writer.Write(MaxEngagementDistance);                // 0x2C
            writer.Write(WpNumber);                             // 0x30
            writer.Write(WAccuracy2);                           // 0x34
            writer.Write(WAccuracy1);                           // 0x36
            writer.Write(Yaw);                                  // 0x38
            writer.Write(Pitch);                                // 0x3A
            writer.Write(Roll);                                 // 0x3C
            writer.Write(Spawns);                               // 0x3E
            writer.Write(CrouchTimer);                          // 0x40
            writer.Write(Unk15a);                               // 0x41
            writer.Write(ShootTimer);                           // 0x42
            writer.Write(WpAdvTrigger);                         // 0x44
            writer.Write(Attention);                            // 0x46
            writer.Write(AlertState);                           // 0x48
            writer.Write(Team);                                 // 0x49
            writer.Write(NoMoreThan);                           // 0x4A
            writer.Write(NoLessThan);                           // 0x4B
            writer.Write(Unk19);                                // 0x4C
            writer.Write(GroupId);                              // 0x4E
            writer.Write(WaypointId);                           // 0x4F
            writer.Write(Obliqueness);                          // 0x50
            writer.Write(MapSymbol);                            // 0x51
            writer.Write(Unk22);                                // 0x52
            writer.Write(Unk23);                                // 0x54
            writer.Write(Unk24);                                // 0x56
            writer.Write(Unk25);                                // 0x58
            writer.Write(Unk26);                                // 0x5A
            writer.Write(FireTimer);                            // 0x5C
            writer.Write(TToolIndex);                           // 0x60
            writer.Write(Unk30_31);                             // 0x64
            writer.Write(Encoding.ASCII.GetBytes(Name1.PadRight(8, '\0')));      // 0x68
            writer.Write(Encoding.ASCII.GetBytes(Name2.PadRight(8, '\0')));      // 0x70
            writer.Write(Encoding.ASCII.GetBytes(GenString.PadRight(36, '\0'))); // 0x78
            writer.Write(MaxAttackDistance);                    // 0x9C
            writer.Write(Unk41);                                // 0xA0
            writer.Write(ColorOverride);                        // 0xA4
            writer.Write(TeamBudget);                           // 0xA5
            writer.Write(Unk42b);                               // 0xA6
            writer.Write(Unk43);                                // 0xA8
        }
    }
}
