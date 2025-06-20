using Godot;
using OpenNova.Core;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace OpenNova.Mission
{
    [GlobalClass]
    public partial class MissionFile : Resource
    {
        private const int WAYPOINT_RECORD_COUNT = 128;
        private const int GROUP_RECORD_COUNT = 64;
        private const int LAYER_RECORD_COUNT = 32;

        [Export] public MissionHeader Header { get; set; }
        [Export] public WeaponLoadout Loadout { get; set; }
        [Export] public Godot.Collections.Array<MissionEntity> Items { get; set; }
        [Export] public Godot.Collections.Array<MissionEntity> Decorations { get; set; }
        [Export] public Godot.Collections.Array<MissionEntity> Markers { get; set; }
        [Export] public Godot.Collections.Array<MissionEntity> Organics { get; set; }
        [Export] public Godot.Collections.Array<MissionWaypoint> WaypointRecords { get; set; }
        [Export] public Godot.Collections.Array<MissionGroup> MissionGroups { get; set; }
        [Export] public Godot.Collections.Array<MissionLayer> MissionLayers { get; set; }
        [Export] public Godot.Collections.Array<MissionEvent> Events { get; set; }
        [Export] public Godot.Collections.Array<MissionTrigger> Triggers { get; set; }
        [Export] public Godot.Collections.Array<MissionAction> Actions { get; set; }
        [Export] public Godot.Collections.Array<BoundingBox> BoundingBoxes { get; set; }
        [Export] public Godot.Collections.Array<AreaTrigger> AreaTriggers { get; set; }

        public int EventsCount { get; private set; }
        public int TriggerCount { get; private set; }
        public int ActionCount { get; private set; }
        public int BoundingBoxCount { get; private set; }

        public static MissionFile Parse(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            using (ONBinaryReader reader = new ONBinaryReader(ms))
            {
                var file = new MissionFile();
                file.Initialize();
                file.Deserialize(reader);
                file.Validate();
                //file.Dump();
                return file;
            }
        }

        public void Initialize()
        {
            Items = new Godot.Collections.Array<MissionEntity>();
            Decorations = new Godot.Collections.Array<MissionEntity>();
            Markers = new Godot.Collections.Array<MissionEntity>();
            Organics = new Godot.Collections.Array<MissionEntity>();
            WaypointRecords = new Godot.Collections.Array<MissionWaypoint>();
            MissionGroups = new Godot.Collections.Array<MissionGroup>();
            MissionLayers = new Godot.Collections.Array<MissionLayer>();
            Events = new Godot.Collections.Array<MissionEvent>();
            Triggers = new Godot.Collections.Array<MissionTrigger>();
            Actions = new Godot.Collections.Array<MissionAction>();
            BoundingBoxes = new Godot.Collections.Array<BoundingBox>();
            AreaTriggers = new Godot.Collections.Array<AreaTrigger>();

        }

        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                // Update header counts from current collections
                Header.NumItems = (uint)Items.Count;
                Header.NumBuildings = (uint)Decorations.Count;
                Header.NumMarkers = (uint)Markers.Count;
                Header.NumPeople = (uint)Organics.Count;
                Header.AreaTriggerCount = (short)AreaTriggers.Count;

                // Serialize header
                Header.Serialize(writer);

                // Serialize weapon loadout
                Loadout.Serialize(writer);

                // Serialize entities in order
                foreach (var item in Items)
                    item.Serialize(writer);

                foreach (var building in Decorations)
                    building.Serialize(writer);

                foreach (var marker in Markers)
                    marker.Serialize(writer);

                foreach (var organic in Organics)
                    organic.Serialize(writer);

                // Serialize waypoint records (always 128)
                for (int i = 0; i < WAYPOINT_RECORD_COUNT; i++)
                {
                    if (i < WaypointRecords.Count)
                        WaypointRecords[i].Serialize(writer);
                    else
                    {
                        // Write empty waypoint
                        var empty = new MissionWaypoint();
                        empty.Initialize();
                        empty.Serialize(writer);
                    }
                }

                // Serialize group records (always 64)
                for (int i = 0; i < GROUP_RECORD_COUNT; i++)
                {
                    if (i < MissionGroups.Count)
                        MissionGroups[i].Serialize(writer);
                    else
                    {
                        // Write empty group
                        var empty = new MissionGroup();
                        empty.Serialize(writer);
                    }
                }

                // Serialize layer records (always 32)
                for (int i = 0; i < LAYER_RECORD_COUNT; i++)
                {
                    if (i < MissionLayers.Count)
                        MissionLayers[i].Serialize(writer);
                    else
                    {
                        // Write empty layer
                        var empty = new MissionLayer();
                        empty.Serialize(writer);
                    }
                }

                // Serialize area triggers
                foreach (var areaTrigger in AreaTriggers)
                    areaTrigger.Serialize(writer);

                // Write event/trigger/action counts
                writer.Write(Events.Count);
                writer.Write(Triggers.Count);
                writer.Write(Actions.Count);

                // Serialize events
                foreach (var evt in Events)
                    evt.Serialize(writer);

                // Serialize triggers
                foreach (var trigger in Triggers)
                    trigger.Serialize(writer);

                // Serialize actions
                foreach (var action in Actions)
                    action.Serialize(writer);

                // Write bounding box count and serialize them
                writer.Write(BoundingBoxes.Count);
                foreach (var bbox in BoundingBoxes)
                    bbox.Serialize(writer);

                return ms.ToArray();
            }
        }

        public void Deserialize(ONBinaryReader reader)
        {
            Header = new MissionHeader();
            Header.Deserialize(reader);

            Loadout = new WeaponLoadout();
            Loadout.Deserialize(reader, Header.WeaponLoadoutChunkLen);

            for (int i = 0; i < Header.NumItems; i++)
            {
                var item = new MissionEntity();
                item.Deserialize(reader);
                item.Type = ItemType.Item;
                Items.Add(item);
            }

            for (int i = 0; i < Header.NumBuildings; i++)
            {
                var item = new MissionEntity();
                item.Deserialize(reader);
                item.Type = ItemType.Building;
                Decorations.Add(item);
            }

            for (int i = 0; i < Header.NumMarkers; i++)
            {
                var item = new MissionEntity();
                item.Deserialize(reader);
                item.Type = ItemType.Marker;
                Markers.Add(item);
            }

            for (int i = 0; i < Header.NumPeople; i++)
            {
                var item = new MissionEntity();
                item.Deserialize(reader);
                item.Type = ItemType.Organic;
                Organics.Add(item);
            }

            for (int i = 0; i < WAYPOINT_RECORD_COUNT; i++)
            {
                var record = new MissionWaypoint();
                record.Initialize();
                record.Deserialize(reader);
                WaypointRecords.Add(record);
            }

            for (int i = 0; i < GROUP_RECORD_COUNT; i++)
            {
                var record = new MissionGroup();
                record.Deserialize(reader);
                MissionGroups.Add(record);
            }

            for (int i = 0; i < LAYER_RECORD_COUNT; i++)
            {
                var record = new MissionLayer();
                record.Deserialize(reader);
                MissionLayers.Add(record);
            }

            for (int i = 0; i < Header.AreaTriggerCount; i++)
            {
                var areaTrigger = new AreaTrigger();
                areaTrigger.Deserialize(reader);
                AreaTriggers.Add(areaTrigger);
            }

            EventsCount = reader.ReadInt32();
            TriggerCount = reader.ReadInt32();
            ActionCount = reader.ReadInt32();

            for (int e = 0; e < EventsCount; e++)
            {
                var eventRecord = new MissionEvent();
                eventRecord.Deserialize(reader);
                Events.Add(eventRecord);
            }

            for (int t = 0; t < TriggerCount; t++)
            {
                var trigger = new MissionTrigger();
                trigger.Deserialize(reader);
                Triggers.Add(trigger);
            }

            for (int a = 0; a < ActionCount; a++)
            {
                var action = new MissionAction();
                action.Deserialize(reader);
                Actions.Add(action);
            }

            BoundingBoxCount = reader.ReadInt32();

            for (int i = 0; i < BoundingBoxCount; i++)
            {
                var boundingBox = new BoundingBox();
                boundingBox.Deserialize(reader);
                BoundingBoxes.Add(boundingBox);
            }

            Debug.Assert(reader.BaseStream.Position == reader.BaseStream.Length, $"At {reader.BaseStream.Position} but is {reader.BaseStream.Length}");
        }

        public void Dump()
        {
            // Get map type name
            var flags = Header.AttribFlags;
            string mapType = "SinglePlayer";
            if (flags.HasFlag(AttribFlags.Deathmatch)) mapType = "Deathmatch";
            else if (flags.HasFlag(AttribFlags.TeamDeathmatch)) mapType = "TeamDeathmatch";
            else if (flags.HasFlag(AttribFlags.Coop)) mapType = "Coop";
            else if (flags.HasFlag(AttribFlags.TeamKingOfTheHill)) mapType = "TeamKingOfTheHill";
            else if (flags.HasFlag(AttribFlags.KingOfTheHill)) mapType = "KingOfTheHill";
            else if (flags.HasFlag(AttribFlags.SearchAndDestroy)) mapType = "SearchAndDestroy";
            else if (flags.HasFlag(AttribFlags.AttackAndDefend)) mapType = "AttackAndDefend";
            else if (flags.HasFlag(AttribFlags.CaptureTheFlag)) mapType = "CaptureTheFlag";
            else if (flags.HasFlag(AttribFlags.FlagBall)) mapType = "FlagBall";
            else if (flags.HasFlag(AttribFlags.AdvanceAndSecure)) mapType = "AdvanceAndSecure";
            else if (flags.HasFlag(AttribFlags.ConquerAndControl)) mapType = "ConquerAndControl";

            GD.Print($"=== {Header.MissionName} by {Header.Designer} ===");
            GD.Print($"Terrain: {Header.Terrain} | Type: {mapType} | Climate: {Header.Climate}");
            GD.Print($"Entities: {Items.Count} items, {Decorations.Count} buildings, {Markers.Count} markers, {Organics.Count} people");
            GD.Print($"Scripts: {Events.Count} events, {Triggers.Count} triggers, {Actions.Count} actions");
            GD.Print($"World: {AreaTriggers.Count} area triggers, {BoundingBoxes.Count} bounding boxes");

            // Detailed script dump in plain text format
            GD.Print("");
            GD.Print("");
            GD.Print("");
            GD.Print("");

            for (int i = 0; i < Events.Count; i++)
            {
                var evt = Events[i];

                GD.Print($"Event {i}: Event {i} Delay={evt.Delay}, ResetAfter={evt.ResetAfter}, Flags={evt.Flags}");
                GD.Print("");

                // Print triggers section if there are any
                if (evt.TriggerCount > 0)
                {
                    GD.Print("Triggers:");
                    GD.Print("");

                    // Build trigger line with logic operators
                    string triggerLine = "    ";
                    for (int t = 0; t < evt.TriggerCount; t++)
                    {
                        var trigger = Triggers[evt.TriggerIndex + t];

                        if (t > 0)
                        {
                            // For subsequent triggers, show the logic operator followed by condition
                            triggerLine += $" {trigger.GetLogicOperator()}\n    {trigger.GetDescription()}";
                        }
                        else
                        {
                            // First trigger just shows the condition
                            triggerLine += trigger.GetDescription();
                        }
                    }

                    GD.Print(triggerLine);
                }

                // Print actions section if there are any
                if (evt.ActionCount > 0)
                {
                    if (evt.TriggerCount > 0)
                        GD.Print("  Actions:");
                    else
                        GD.Print("  Actions:");

                    GD.Print("");

                    for (int a = 0; a < evt.ActionCount; a++)
                    {
                        var action = Actions[evt.ActionIndex + a];
                        GD.Print($"    {action.GetDescription()}");
                        GD.Print($"        Raw: {action.GetRawHex()}");
                    }
                }

                GD.Print("");
            }

            // Waypoint summary
            var activeWaypoints = WaypointRecords.Where(w => w.Flags != MissionWaypointFlags.None || w.MarkerCount > 0).ToList();
            if (activeWaypoints.Count > 0)
            {
                GD.Print($"Active Waypoints: {activeWaypoints.Count}");
                foreach (var wp in activeWaypoints.Take(5))
                {
                    GD.Print($"  Flags: {wp.Flags}, Markers: {wp.MarkerCount}, Path: [{string.Join(",", wp.WaypointNumbers)}]");
                }
                if (activeWaypoints.Count > 5) GD.Print($"  ... and {activeWaypoints.Count - 5} more");
            }

            // Unknown data summary
            var unknownCollections = new[] {
                ($"UnkRec1[{MissionGroups.Count}]", MissionGroups.Count > 0 ? MissionGroups[0].RawData : null),
                ($"UnkRec2[{MissionLayers.Count}]", MissionLayers.Count > 0 ? MissionLayers[0].RawData : null)
            };

            foreach (var (name, data) in unknownCollections)
            {
                if (data != null && data.Any(b => b != 0))
                {
                    var preview = string.Join(" ", data.Take(8).Select(b => $"{b:X2}"));
                    GD.Print($"{name}: {preview}{(data.Length > 8 ? "..." : "")}");
                }
                else if (data != null)
                {
                    GD.Print($"{name}: [all zeros]");
                }
            }

            // Header unknown data
            var headerUnknowns = new[] {
                ("Unk0", Header.Unknown0), ("Unk3", Header.Unknown3), ("Unk4", Header.Unknown4),
                ("Unk5", Header.Unknown5), ("Unk7", Header.Unknown7), ("Unk9", Header.Unknown9)
            };

            foreach (var (name, data) in headerUnknowns)
            {
                if (data?.Any(b => b != 0) == true)
                {
                    var preview = string.Join(" ", data.Take(6).Select(b => $"{b:X2}"));
                    GD.Print($"Header.{name}[{data.Length}]: {preview}{(data.Length > 6 ? "..." : "")}");
                }
            }

            // Area triggers preview
            if (AreaTriggers.Count > 0)
            {
                GD.Print($"Area Triggers:");
                foreach (var trigger in AreaTriggers.Take(3))
                {
                    GD.Print($"  WP{trigger.WpNumber}: ({trigger.MinX:F1},{trigger.MinY:F1},{trigger.MinZ:F1}) to ({trigger.MaxX:F1},{trigger.MaxY:F1},{trigger.MaxZ:F1})");
                }
                if (AreaTriggers.Count > 3) GD.Print($"  ... and {AreaTriggers.Count - 3} more");
            }

            // Bounding boxes preview  
            if (BoundingBoxes.Count > 0)
            {
                GD.Print($"Bounding Boxes:");
                foreach (var bbox in BoundingBoxes.Take(2))
                {
                    var hasUnknown = bbox.UnknownData?.Any(b => b != 0) == true;
                    var unknownPreview = hasUnknown ? $" [unk: {string.Join(" ", bbox.UnknownData.Take(4).Select(b => $"{b:X2}"))}]" : "";
                    GD.Print($"  ({bbox.MinX:F1},{bbox.MinY:F1},{bbox.MinZ:F1}) to ({bbox.MaxX:F1},{bbox.MaxY:F1},{bbox.MaxZ:F1}){unknownPreview}");
                }
                if (BoundingBoxes.Count > 2) GD.Print($"  ... and {BoundingBoxes.Count - 2} more");
            }

            GD.Print("=== End Dump ===");
        }

        private void Validate()
        {
            // Sanity checks during reversing
            int computedTriggerCount = 0;
            foreach (var e in Events)
            {
                computedTriggerCount += e.TriggerCount;
            }
            Debug.Assert(computedTriggerCount == TriggerCount, $"Trigger count mismatch got: {computedTriggerCount} wanted: {TriggerCount}");

            int computedActionCount = 0;
            foreach (var e in Events)
            {
                computedActionCount += e.ActionCount;
            }
            Debug.Assert(computedActionCount == ActionCount, $"Action count mismatch got: {computedActionCount} wanted: {ActionCount}");

        }
    }
}
