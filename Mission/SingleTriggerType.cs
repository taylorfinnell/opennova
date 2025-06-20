namespace OpenNova.Mission
{
    public enum SingleTriggerType
    {
        Null = 0,
        SingleSeesGroup = 1,
        SingleHasTargetedGroup = 2,
        SingleAtRedAlert = 3,
        SingleDestroyed = 4,
        SingleAlive = 5,
        SingleHasLostMoreUnits = 6,
        SingleAtWaypoint = 7,
        SingleIntact = 9,
        SingleIsWithinArea = 10,
        SingleHoldingGroup = 11,
        SingleHasMoreUnits = 12,
        SingleHasShotGroup = 13,
        SingleAtYellowAlert = 14,
        SingleHasTargetedSingle = 15,
        SingleSeesSingle = 16,
        SingleHasShotSingle = 17,
        SingleOnTopOf = 42,
        SingleFartherThan = 43,
        SingleHasNoLOS = 44,
        SingleDoesNotSeeOrFarther = 45
    }
}
