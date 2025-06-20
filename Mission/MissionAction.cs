using Godot;
using OpenNova.Core;
using System.Diagnostics;
using System.IO;

namespace OpenNova.Mission
{

    [Tool]
    [GlobalClass]
    public partial class MissionAction : Resource
    {
        public int Reserved0 { get; set; }
        [Export] public ActionType ActionType { get; set; }
        [Export] public int ActionSubType { get; set; }
        [Export] public int Param1 { get; set; }
        [Export] public int Param2 { get; set; }
        [Export] public int Param3 { get; set; }
        [Export] public int Param4 { get; set; }
        public int Reserved1 { get; set; }

        public void Deserialize(ONBinaryReader reader)
        {
            reader.MarkPosition();
            Reserved0 = reader.ReadInt32();
            Debug.Assert(Reserved0 == 0);

            ActionType = (ActionType)reader.ReadInt32();
            ActionSubType = reader.ReadInt32();
            Param1 = reader.ReadInt32();
            Param2 = reader.ReadInt32();
            Param3 = reader.ReadInt32();
            Param4 = reader.ReadInt32();

            Reserved1 = reader.ReadInt32();
            Debug.Assert(Reserved1 == 0);

            reader.AssertBytesRead(32);
        }

        public string GetRawHex() =>
            $"{Reserved0:X8} {(int)ActionType:X8} {ActionSubType:X8} {Param1:X8} {Param2:X8} {Param3:X8} {Param4:X8} {Reserved1:X8}";

        public string GetDescription()
        {
            switch (ActionType)
            {
                case ActionType.Null:
                    return "No action";
                case ActionType.RedirectGroupTo:
                    return Param3 == -1
                        ? $"Redirect group {Param1} to nearest waypoint in list {Param2}"
                        : $"Redirect group {Param1} to waypoint {Param2} - {Param3}";
                case ActionType.KillGroup:
                    return $"Kill Group {Param1}";
                case ActionType.ChangeGroupAI:
                    return (AIActionSubType)ActionSubType switch
                    {
                        AIActionSubType.GuardBit => $"Change Group {Param1} AI: guard bit",
                        AIActionSubType.Accuracy => $"Change Group {Param1} AI: accuracy to {Param2}",
                        AIActionSubType.BlindBit when Param2 == 1 => $"Change Group {Param1} AI: blind",
                        AIActionSubType.BlindBit when Param2 == 0 => $"Change Group {Param1} AI: not blind",
                        AIActionSubType.BerserkBit => $"Change Group {Param1} AI: berserk bit",
                        AIActionSubType.ClimberBit => $"Change Group {Param1} AI: climber bit",
                        AIActionSubType.CowardBit => $"Change Group {Param1} AI: coward bit",
                        AIActionSubType.Skill1 => $"Change Group {Param1} AI: skill level {Param2}",
                        AIActionSubType.Skill2 => $"Change Group {Param1} AI: skill level {Param2}",
                        AIActionSubType.AiState => $"Change Group {Param1} AI: AI state {Param2}",
                        AIActionSubType.SpeedKmh1 => $"Change Group {Param1} AI: speed = {Param2} km/h",
                        AIActionSubType.SpeedKmh2 => $"Change Group {Param1} AI: patrol speed = {Param2} km/h",
                        AIActionSubType.TargetSsn1 => $"Change Group {Param1} AI: target SSN {Param2}",
                        AIActionSubType.AnimNum => $"Change Group {Param1} AI: animation {Param2}",
                        AIActionSubType.HudItem => $"Change Group {Param1} AI: HUD item {Param2}",
                        AIActionSubType.TmateStatus => $"Change Group {Param1} AI: teammate status {Param2}",
                        AIActionSubType.AiNodePathBit => $"Change Group {Param1} AI: AI node path bit",
                        AIActionSubType.AttackDistanceValue => $"Change Group {Param1} AI: attack distance {Param2}",
                        AIActionSubType.EngageDistanceMin => $"Change Group {Param1} AI: engage distance {Param2}",
                        AIActionSubType.IndestructableBit => $"Change Group {Param1} AI: destroyable",
                        AIActionSubType.TargetSsn2 => $"Change Group {Param1} AI: target SSN {Param2}",
                        AIActionSubType.StartFiringBit => $"Change Group {Param1} AI: start firing bit",
                        AIActionSubType.FiringAngle => $"Change Group {Param1} AI: firing angle {Param2}",
                        _ => $"Change Group {Param1} AI: type {ActionSubType} value {Param2}"
                    };
                case ActionType.VaporizeGroup:
                    return $"Vaporize Group {Param1}";
                case ActionType.MisvarChange:
                    return (MissionVariableActionSubType)ActionSubType switch
                    {
                        MissionVariableActionSubType.Null => "Mission variable null action",
                        MissionVariableActionSubType.Set => $"Set Mission Var#{Param1} to {Param2}",
                        MissionVariableActionSubType.Add => $"Add {Param2} to Mission Var#{Param1}",
                        MissionVariableActionSubType.Subtract => $"Subtract {Param2} from Mission Var#{Param1}",
                        MissionVariableActionSubType.Increment => $"Increment Mission Var#{Param1}",
                        MissionVariableActionSubType.Decrement => $"Decrement Mission Var#{Param1}",
                        _ => $"Mission Var#{Param1} action {ActionSubType} with {Param2}"
                    };
                case ActionType.OutputText:
                    return $"Output text {Param1}";
                case ActionType.PlayWavList:
                    return $"Play dialog ID: [{Param1:D2} undefined] {(Param2 == 1 ? "(Plays after end)" : "")}";
                case ActionType.BlueWin:
                    return "Win, Blue";
                case ActionType.RedWin:
                    return "Win, Red";
                case ActionType.GreenWin:
                    return "Win, Green";
                case ActionType.GroupVelocity:
                    return $"Set velocity of Group {Param1} to {Param2}";
                case ActionType.AreaAiRed:
                    return $"Set area {Param1} AI to RED";
                case ActionType.AreaAiBlue:
                    return $"Set area {Param1} AI to BLUE";
                case ActionType.SubGoalWon:
                    return $"Win Sub Goal {Param1} [Mission Critical]";
                case ActionType.SubGoalLost:
                    return $"Lose Sub Goal {Param1} [Mission Critical]";
                case ActionType.ChangeGTeamAction:
                    return $"Change GTeam action {ActionSubType} for Group {Param1} to {Param2}";
                case ActionType.ChangeGroupAction:
                    return $"Change Group action {ActionSubType} for Group {Param1} to {Param2}";
                case ActionType.GroupTeleportAction:
                    return $"Teleport Group {Param1} to SSN {Param2}";
                case ActionType.RedirectSingleTo:
                    return Param3 == 0
                        ? $"Redirect SSN {Param1} to waypoint {Param2}-{Param3}"
                        : $"Redirect SSN {Param1} to waypoint {Param2} - {Param3}";
                case ActionType.KillSingle:
                    return $"Kill SSN {Param1}";
                case ActionType.ChangeSingleAI:
                    return (AIActionSubType)ActionSubType switch
                    {
                        AIActionSubType.GuardBit => $"Change SSN {Param1} AI: guard bit",
                        AIActionSubType.Accuracy => $"Change SSN {Param1} AI: accuracy to {Param2}",
                        AIActionSubType.BlindBit when Param2 == 1 => $"Change SSN {Param1} AI: blind",
                        AIActionSubType.BlindBit when Param2 == 0 => $"Change SSN {Param1} AI: not blind",
                        AIActionSubType.BerserkBit => $"Change SSN {Param1} AI: berserk bit",
                        AIActionSubType.ClimberBit => $"Change SSN {Param1} AI: climber bit",
                        AIActionSubType.CowardBit => $"Change SSN {Param1} AI: coward bit",
                        AIActionSubType.Skill1 => $"Change SSN {Param1} AI: skill level {Param2}",
                        AIActionSubType.Skill2 => $"Change SSN {Param1} AI: skill level {Param2}",
                        AIActionSubType.AiState => $"Change SSN {Param1} AI: AI state {Param2}",
                        AIActionSubType.SpeedKmh1 => $"Change SSN {Param1} AI: speed = {Param2} km/h",
                        AIActionSubType.SpeedKmh2 => $"Change SSN {Param1} AI: patrol speed = {Param2} km/h",
                        AIActionSubType.TargetSsn1 => $"Change SSN {Param1} AI: target SSN {Param2}",
                        AIActionSubType.AnimNum => $"Change SSN {Param1} AI: animation {Param2}",
                        AIActionSubType.HudItem => $"Change SSN {Param1} AI: HUD item {Param2}",
                        AIActionSubType.TmateStatus => $"Change SSN {Param1} AI: teammate status {Param2}",
                        AIActionSubType.AiNodePathBit => $"Change SSN {Param1} AI: AI node path bit",
                        AIActionSubType.AttackDistanceValue => $"Change SSN {Param1} AI: attack distance {Param2}",
                        AIActionSubType.EngageDistanceMin => $"Change SSN {Param1} AI: engage distance {Param2}",
                        AIActionSubType.IndestructableBit => $"Change SSN {Param1} AI: destroyable",
                        AIActionSubType.TargetSsn2 => $"Change SSN {Param1} AI: target SSN {Param2}",
                        AIActionSubType.StartFiringBit => $"Change SSN {Param1} AI: start firing bit",
                        AIActionSubType.FiringAngle => $"Change SSN {Param1} AI: firing angle {Param2}",
                        _ => $"Change SSN {Param1} AI: type {ActionSubType} value {Param2}"
                    };
                case ActionType.VaporizeSingle:
                    return $"Vaporize SSN {Param1}";
                case ActionType.SingleVelocity:
                    return $"Set velocity of SSN {Param1} to {Param2}";
                case ActionType.ChangeSteamAction:
                    return $"Change SSN {Param1} steam action {ActionSubType} to {Param2}";
                case ActionType.SingleChangeGroup:
                    return $"Change SSN {Param1} to Group {Param2}";
                case ActionType.SingleTeleportAction:
                    return $"Teleport SSN {Param1} to {Param2}";
                case ActionType.ParticleEffectAction:
                    return $"Particle effect {ActionSubType} at SSN {Param1}";
                case ActionType.GroupOpenDoorAction:
                    return $"Group {Param1} open door {Param2}";
                case ActionType.GroupCloseDoorAction:
                    return $"Group {Param1} close door {Param2}";
                case ActionType.GroupResetHasVisited:
                    return $"Reset visited flags for Group {Param1}";
                case ActionType.SingleResetHasVisited:
                    return $"Reset visited flags for SSN {Param1}";
                case ActionType.ResetEvent:
                    return $"Reset Event {Param1}";
                case ActionType.ShowWinSubgoal:
                    return Param2 == 1
                        ? $"Show Win Subgoal {Param1}"
                        : $"Hide Win Subgoal {Param1}";
                case ActionType.ShowLoseSubgoal:
                    return $"Show Lose Subgoal {Param1}";
                case ActionType.AttachToEmplaced:
                    return $"Attach SSN {Param1} to emplaced {Param2}";
                case ActionType.SetLightState:
                    return $"Set light state {ActionSubType} in area {Param1} to {Param2}";
                case ActionType.Teammates:
                    return (TeammateActionSubType)ActionSubType switch
                    {
                        TeammateActionSubType.Null => "Teammate null action",
                        TeammateActionSubType.MedicAssist => $"Teammate medic assist Group {Param1} value {Param2}",
                        TeammateActionSubType.EvacuateTt => $"Teammate evacuate TT Group {Param1} to {Param2}",
                        TeammateActionSubType.EvacuateAt => $"Teammate evacuate AT Group {Param1} to {Param2}",
                        _ => $"Teammate action {ActionSubType} for Group {Param1} to {Param2}"
                    };
                case ActionType.ShowWaypoints:
                    return "Hide Waypoints";
                case ActionType.ExecuteWac:
                    return $"Execute WAC command {ActionSubType} for Group {Param1}";
                case ActionType.SsnTargetSsnPri:
                    return $"SSN {Param1} prioritize target SSN {Param2}";
                case ActionType.SsnTargetSsnExc:
                    return $"SSN {Param1} exclusively target SSN {Param2}";
                case ActionType.SsnTargetGroupPri:
                    return $"SSN {Param1} prioritize target Group {Param2}";
                case ActionType.SsnTargetGroupExc:
                    return $"SSN {Param1} exclusively target Group {Param2}";
                case ActionType.GroupTargetSsnPri:
                    return $"Group {Param1} prioritize target SSN {Param2}";
                case ActionType.GroupTargetSsnExc:
                    return $"Group {Param1} exclusively target SSN {Param2}";
                case ActionType.GroupTargetGroupPri:
                    return $"Group {Param1} prioritize target Group {Param2}";
                case ActionType.GroupTargetGroupExc:
                    return $"Group {Param1} exclusively target Group {Param2}";
                default:
                    return $"Unknown Action {ActionType} code {ActionType} params({ActionSubType},{Param1},{Param2},{Param3},{Param4})";
            }
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Reserved0);
            writer.Write((int)ActionType);
            writer.Write(ActionSubType);
            writer.Write(Param1);
            writer.Write(Param2);
            writer.Write(Param3);
            writer.Write(Param4);
            writer.Write(Reserved1);
        }
    }
}
