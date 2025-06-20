using Godot;
using OpenNova.Core;
using System.Diagnostics;
using System.IO;

namespace OpenNova.Mission
{

    [GlobalClass]
    public partial class MissionEvent : Resource
    {
        [Export] public EventFlags Flags { get; set; }
        [Export] public int TriggerIndex { get; set; }
        [Export] public int ActionIndex { get; set; }
        [Export] public int ResetAfter { get; set; }
        [Export] public int Delay { get; set; }
        public byte Unknown5 { get; set; }
        [Export] public byte TriggerCount { get; set; }
        [Export] public byte ActionCount { get; set; }
        public byte Unknown6 { get; set; }

        public void Deserialize(ONBinaryReader reader)
        {
            reader.MarkPosition();
            var flagsValue = reader.ReadInt32();
            Flags = (EventFlags)flagsValue;
            TriggerIndex = reader.ReadInt32();
            ActionIndex = reader.ReadInt32();

            var resetAfterRaw = reader.ReadInt32();
            var delayRaw = reader.ReadInt32();

            ResetAfter = resetAfterRaw >> 22;
            Delay = delayRaw >> 22;

            Debug.Assert((resetAfterRaw & 0x3FFFFF) == 0, $"ResetAfter lower 22 bits not zero: {resetAfterRaw & 0x3FFFFF:X}");
            Debug.Assert((delayRaw & 0x3FFFFF) == 0, $"Delay lower 22 bits not zero: {delayRaw & 0x3FFFFF:X}");

            Debug.Assert(ResetAfter >= 0 && ResetAfter <= 1023, $"ResetAfter out of range: {ResetAfter}");
            Debug.Assert(Delay >= 0 && Delay <= 1023, $"Delay out of range: {Delay}");

            Unknown5 = reader.ReadByte();
            TriggerCount = reader.ReadByte();
            ActionCount = reader.ReadByte();
            Unknown6 = reader.ReadByte();

            reader.AssertBytesRead(24);

            var validFlags = EventFlags.None | EventFlags.ResetAfter | EventFlags.PreMission | EventFlags.PostMission | EventFlags.Unknown4 | EventFlags.Unknown5;
            Debug.Assert((flagsValue & ~(int)validFlags) == 0, $"Unknown event flags: {flagsValue & ~(int)validFlags}");

            Debug.Assert(Unknown5 == 0, $"Unknown5 expected 0, got {Unknown5}");
            Debug.Assert(Unknown6 == 0, $"Unknown6 expected 0, got {Unknown6}");
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((int)Flags);
            writer.Write(TriggerIndex);
            writer.Write(ActionIndex);

            // Pack reset after and delay values with 22-bit shift
            writer.Write(ResetAfter << 22);
            writer.Write(Delay << 22);

            writer.Write(Unknown5);
            writer.Write(TriggerCount);
            writer.Write(ActionCount);
            writer.Write(Unknown6);
        }
    }
}
