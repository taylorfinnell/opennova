using Godot;
using OpenNova.Core;
using System.Diagnostics;
using System.IO;

namespace OpenNova.Mission
{

    [GlobalClass]
    public partial class MissionTrigger : Resource
    {
        [Export] public int ConditionFlags { get; set; }
        [Export] public TriggerMainType MainType { get; set; }
        [Export] public int SubType { get; set; }
        [Export] public int Parameter1 { get; set; }
        [Export] public int Parameter2 { get; set; }
        [Export] public int Parameter3 { get; set; }
        [Export] public int Parameter4 { get; set; }
        public int Unknown7 { get; set; }

        public bool IsNegated => (ConditionFlags & 1) != 0;
        public bool IsOr => (ConditionFlags & 2) != 0;
        public bool IsXor => (ConditionFlags & 4) != 0;

        public void Deserialize(ONBinaryReader reader)
        {
            reader.MarkPosition();
            ConditionFlags = reader.ReadInt32();
            MainType = (TriggerMainType)reader.ReadInt32();
            SubType = reader.ReadInt32();
            Parameter1 = reader.ReadInt32();
            Parameter2 = reader.ReadInt32();
            Parameter3 = reader.ReadInt32();
            Parameter4 = reader.ReadInt32();
            Unknown7 = reader.ReadInt32();
            Debug.Assert(Unknown7 == 0);
            reader.AssertBytesRead(32);
        }

        public string GetRawHex() =>
            $"{ConditionFlags:X8} {(int)MainType:X8} {SubType:X8} {Parameter1:X8} {Parameter2:X8} {Parameter3:X8} {Parameter4:X8} {Unknown7:X8}";

        public string GetDescription() => GetTriggerCondition();

        private string GetTriggerCondition()
        {
            switch (MainType)
            {
                case TriggerMainType.Group:
                    return GetGroupTrigger();
                case TriggerMainType.Single:
                    return GetSingleTrigger();
                case TriggerMainType.Event:
                    return IsNegated
                        ? $"Event {Parameter1} has not been triggered"
                        : $"Event {Parameter1} has been triggered";
                case TriggerMainType.MissionVariable:
                    return GetMissionVariableTrigger();
                case TriggerMainType.SecondTimeThrough:
                    return IsNegated
                        ? "Not second time through"
                        : "Second time through";
                case TriggerMainType.Teammate:
                    return GetTeammateTrigger();
                case TriggerMainType.Player:
                    return GetPlayerTrigger();
                default:
                    return $"Type {(int)MainType} SubType {SubType} params({Parameter1},{Parameter2},{Parameter3},{Parameter4})";
            }
        }

        private string GetGroupTrigger()
        {
            switch ((GroupTriggerType)SubType)
            {
                case GroupTriggerType.Null:
                    return "Group-NULL";
                case GroupTriggerType.GroupSeesGroup:
                    return IsNegated
                        ? $"Group {Parameter1} does not see Group {Parameter2}"
                        : $"Group {Parameter1} sees Group {Parameter2}";
                case GroupTriggerType.GroupHasTargetedGroup:
                    return IsNegated
                        ? $"Group {Parameter1} has not targeted Group {Parameter2}"
                        : $"Group {Parameter1} has targeted Group {Parameter2}";
                case GroupTriggerType.GroupAtRedAlert:
                    return IsNegated
                        ? $"Group {Parameter1} not at red alert"
                        : $"Group {Parameter1} at red alert";
                case GroupTriggerType.GroupDestroyed:
                    return IsNegated
                        ? $"Group {Parameter1} has not been destroyed"
                        : $"Group {Parameter1} has been destroyed";
                case GroupTriggerType.GroupAlive:
                    return IsNegated
                        ? $"Group {Parameter1} not alive"
                        : $"Group {Parameter1} alive";
                case GroupTriggerType.GroupHasLostMoreUnits:
                    return IsNegated
                        ? $"Group {Parameter1} has not lost {Parameter2} or more units"
                        : $"Group {Parameter1} has lost {Parameter2} or more units";
                case GroupTriggerType.GroupAtWaypoint:
                    return IsNegated
                        ? $"Group {Parameter1} has not reached waypoint {Parameter2}, {Parameter3}"
                        : $"Group {Parameter1} reaches waypoint {Parameter2}, {Parameter3}";
                case GroupTriggerType.GroupIntact:
                    return IsNegated
                        ? $"Group {Parameter1} not intact"
                        : $"Group {Parameter1} intact";
                case GroupTriggerType.GroupIsWithinArea:
                    return IsNegated
                        ? $"Group {Parameter1} is not within area {Parameter2}"
                        : $"Group {Parameter1} is within area {Parameter2}";
                case GroupTriggerType.GroupHoldingGroup:
                    return IsNegated
                        ? $"Group {Parameter1} not holding Group {Parameter2}"
                        : $"Group {Parameter1} holding Group {Parameter2}";
                case GroupTriggerType.GroupHasMoreUnits:
                    return IsNegated
                        ? $"Group {Parameter1} has less than {Parameter2} units"
                        : $"Group {Parameter1} has more than {Parameter2} units";
                case GroupTriggerType.GroupHasShotGroup:
                    return IsNegated
                        ? $"Group {Parameter1} has not shot Group {Parameter2}"
                        : $"Group {Parameter1} has shot Group {Parameter2}";
                case GroupTriggerType.GroupAtYellowAlert:
                    return IsNegated
                        ? $"Group {Parameter1} not at yellow alert"
                        : $"Group {Parameter1} at yellow alert";
                case GroupTriggerType.GroupHasTargetedSingle:
                    return IsNegated
                        ? $"Group {Parameter1} has not targeted SSN {Parameter2}"
                        : $"Group {Parameter1} has targeted SSN {Parameter2}";
                case GroupTriggerType.GroupSeesSingle:
                    return IsNegated
                        ? $"Group {Parameter1} does not see SSN {Parameter2}"
                        : $"Group {Parameter1} sees SSN {Parameter2}";
                case GroupTriggerType.GroupHasShotSingle:
                    return IsNegated
                        ? $"Group {Parameter1} has not shot SSN {Parameter2}"
                        : $"Group {Parameter1} has shot SSN {Parameter2}";
                default:
                    return $"Group trigger subtype {SubType} params({Parameter1},{Parameter2},{Parameter3},{Parameter4})";
            }
        }

        private string GetSingleTrigger()
        {
            switch ((SingleTriggerType)SubType)
            {
                case SingleTriggerType.Null:
                    return "SSN-NULL";
                case SingleTriggerType.SingleSeesGroup:
                    return IsNegated
                        ? $"SSN {Parameter1} does not see Group {Parameter2}"
                        : $"SSN {Parameter1} sees Group {Parameter2}";
                case SingleTriggerType.SingleHasTargetedGroup:
                    return IsNegated
                        ? $"SSN {Parameter1} has not targeted Group {Parameter2}"
                        : $"SSN {Parameter1} has targeted Group {Parameter2}";
                case SingleTriggerType.SingleAtRedAlert:
                    return IsNegated
                        ? $"SSN {Parameter1} not at red alert"
                        : $"SSN {Parameter1} at red alert";
                case SingleTriggerType.SingleDestroyed:
                    return IsNegated
                        ? $"SSN {Parameter1} is not dead"
                        : $"SSN {Parameter1} is dead";
                case SingleTriggerType.SingleAlive:
                    return IsNegated
                        ? $"SSN {Parameter1} not alive"
                        : $"SSN {Parameter1} alive";
                case SingleTriggerType.SingleHasLostMoreUnits:
                    return IsNegated
                        ? $"SSN {Parameter1} has not lost {Parameter2} or more units"
                        : $"SSN {Parameter1} has lost {Parameter2} or more units";
                case SingleTriggerType.SingleAtWaypoint:
                    return IsNegated
                        ? $"SSN {Parameter1} has not reached waypoint {Parameter2}, {Parameter3}"
                        : $"SSN {Parameter1} reaches waypoint {Parameter2}, {Parameter3}";
                case SingleTriggerType.SingleIntact:
                    return IsNegated
                        ? $"SSN {Parameter1} not intact"
                        : $"SSN {Parameter1} intact";
                case SingleTriggerType.SingleIsWithinArea:
                    return IsNegated
                        ? $"SSN {Parameter1} is not within area {Parameter2}"
                        : $"SSN {Parameter1} is within area {Parameter2}";
                case SingleTriggerType.SingleHoldingGroup:
                    return IsNegated
                        ? $"SSN {Parameter1} not holding Group {Parameter2}"
                        : $"SSN {Parameter1} holding Group {Parameter2}";
                case SingleTriggerType.SingleHasMoreUnits:
                    return IsNegated
                        ? $"SSN {Parameter1} has less than {Parameter2} units"
                        : $"SSN {Parameter1} has more than {Parameter2} units";
                case SingleTriggerType.SingleHasShotGroup:
                    return IsNegated
                        ? $"SSN {Parameter1} has not shot Group {Parameter2}"
                        : $"SSN {Parameter1} has shot Group {Parameter2}";
                case SingleTriggerType.SingleAtYellowAlert:
                    return IsNegated
                        ? $"SSN {Parameter1} not at yellow alert"
                        : $"SSN {Parameter1} at yellow alert";
                case SingleTriggerType.SingleHasTargetedSingle:
                    return IsNegated
                        ? $"SSN {Parameter1} has not targeted SSN {Parameter2}"
                        : $"SSN {Parameter1} has targeted SSN {Parameter2}";
                case SingleTriggerType.SingleSeesSingle:
                    return IsNegated
                        ? $"SSN {Parameter1} does not see SSN {Parameter2}"
                        : $"SSN {Parameter1} sees SSN {Parameter2}";
                case SingleTriggerType.SingleHasShotSingle:
                    return IsNegated
                        ? $"SSN {Parameter1} has not shot SSN {Parameter2}"
                        : $"SSN {Parameter1} has shot SSN {Parameter2}";
                case SingleTriggerType.SingleOnTopOf:
                    return IsNegated
                        ? $"SSN {Parameter1} not on top of SSN {Parameter2}"
                        : $"SSN {Parameter1} on top of SSN {Parameter2}";
                case SingleTriggerType.SingleFartherThan:
                    return IsNegated
                        ? $"SSN {Parameter1} is farther than {Parameter3}m from SSN {Parameter2}"
                        : $"SSN {Parameter1} is within {Parameter3}m from SSN {Parameter2}";
                case SingleTriggerType.SingleHasNoLOS:
                    return IsNegated
                        ? $"SSN {Parameter1} has no LOS to SSN {Parameter2} or is farther than {Parameter3}m"
                        : $"SSN {Parameter1} has LOS to SSN {Parameter2} and is within {Parameter3}m";
                case SingleTriggerType.SingleDoesNotSeeOrFarther:
                    return IsNegated
                        ? $"SSN {Parameter1} doesn’t see SSN {Parameter2} or is farther than {Parameter3}m"
                        : $"SSN {Parameter1} sees SSN {Parameter2} and is within {Parameter3}m";
                default:
                    return $"Single trigger subtype {SubType} params({Parameter1},{Parameter2},{Parameter3},{Parameter4})";
            }
        }

        private string GetPlayerTrigger()
        {
            switch ((PlayerTriggerType)SubType)
            {
                case PlayerTriggerType.PlayerBerserk:
                    return IsNegated ? "Player not berserk" : "Player berserk";
                case PlayerTriggerType.PlayerFirstPerson:
                    return IsNegated ? "Player not in first person" : "Player in first person";
                case PlayerTriggerType.PlayerThirdPerson:
                    return IsNegated ? "Player not in third person" : "Player in third person";
                case PlayerTriggerType.PlayerCockpitView:
                    return IsNegated ? "Player not in cockpit view" : "Player in cockpit view";
                case PlayerTriggerType.PlayerDialogDone:
                    return IsNegated
                        ? $"Player dialog {Parameter1} not done"
                        : $"Player dialog {Parameter1} done";
                case PlayerTriggerType.PlayerDialogFinished:
                    return IsNegated
                        ? $"Player dialog {Parameter1} not finished"
                        : $"Player dialog {Parameter1} finished";
                case PlayerTriggerType.PlayerAwol:
                    return IsNegated
                        ? $"Player has not been outside of mission area for {Parameter1} seconds"
                        : $"Player has been outside of mission area for {Parameter1} seconds";
                case PlayerTriggerType.PlayerSatchel:
                    return IsNegated
                        ? $"Player not using satchel {Parameter1}"
                        : $"Player using satchel {Parameter1}";
                case PlayerTriggerType.PlayerAttachedToSsn:
                    return IsNegated
                        ? $"Player Not Attached to SSN {Parameter1}"
                        : $"Player Attached to SSN {Parameter1}";
                case PlayerTriggerType.PlayerOnSsn:
                    return IsNegated
                        ? $"Player Not On SSN {Parameter1}"
                        : $"Player On SSN {Parameter1}";
                case PlayerTriggerType.PlayerDrivingSsn:
                    return IsNegated
                        ? $"Player Not Driving SSN {Parameter1}"
                        : $"Player Driving SSN {Parameter1}";
                case PlayerTriggerType.PlayerOnGun:
                    return IsNegated
                        ? $"Player Not On Gun (SSN {Parameter1})"
                        : $"Player On Gun (SSN {Parameter1})";
                default:
                    return $"Player trigger subtype {SubType} params({Parameter1},{Parameter2},{Parameter3},{Parameter4})";
            }
        }

        private string GetMissionVariableTrigger()
        {
            switch ((MissionVariableTriggerType)SubType)
            {
                case MissionVariableTriggerType.MissionVariableIsEqual:
                    return IsNegated
                        ? $"Mission Var#{Parameter1} is not equal to {Parameter2}"
                        : $"Mission Var#{Parameter1} is equal to {Parameter2}";
                case MissionVariableTriggerType.MissionVariableIsLessThan:
                    return IsNegated
                        ? $"Mission Var#{Parameter1} is greater than or equal to {Parameter2}"
                        : $"Mission Var#{Parameter1} is less than {Parameter2}";
                case MissionVariableTriggerType.MissionVariableIsLessThanOrEqual:
                    return IsNegated
                        ? $"Mission Var#{Parameter1} is greater than {Parameter2}"
                        : $"Mission Var#{Parameter1} is less than or equal to {Parameter2}";
                case MissionVariableTriggerType.MissionVariableIsGreaterThan:
                    return IsNegated
                        ? $"Mission Var#{Parameter1} is less than or equal to {Parameter2}"
                        : $"Mission Var#{Parameter1} is greater than {Parameter2}";
                case MissionVariableTriggerType.MissionVariableIsGreaterThanOrEqual:
                    return IsNegated
                        ? $"Mission Var#{Parameter1} is less than {Parameter2}"
                        : $"Mission Var#{Parameter1} is greater than or equal to {Parameter2}";
                default:
                    return $"Mission Variable trigger subtype {SubType} params({Parameter1},{Parameter2},{Parameter3},{Parameter4})";
            }
        }

        private string GetTeammateTrigger()
        {
            switch ((TeammateTriggerType)SubType)
            {
                case TeammateTriggerType.TeammateIsEnabled:
                    return IsNegated ? "Teammate is disabled" : "Teammate is enabled";
                case TeammateTriggerType.TeammateMedicAssisting:
                    return IsNegated ? "Teammate medic not assisting" : "Teammate medic assisting";
                case TeammateTriggerType.TeammateEvacuating:
                    return IsNegated ? "Teammate not evacuating" : "Teammate evacuating";
                default:
                    return $"Teammate trigger subtype {SubType} params({Parameter1},{Parameter2},{Parameter3},{Parameter4})";
            }
        }

        public string GetLogicOperator()
        {
            if (IsOr) return "or";
            if (IsXor) return "xor";
            return "and";
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(ConditionFlags);
            writer.Write((int)MainType);
            writer.Write(SubType);
            writer.Write(Parameter1);
            writer.Write(Parameter2);
            writer.Write(Parameter3);
            writer.Write(Parameter4);
            writer.Write(Unknown7);
        }
    }
}
