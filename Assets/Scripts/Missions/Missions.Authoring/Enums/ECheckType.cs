namespace Missions.Missions.Authoring
{
    public enum ECheckType : byte
    {
        GreaterOrEqual = 0,
        GreaterThan = 1,
        LessOrEqual = 2,
        LessThan = 3,
        Equals = 4,
        NotEqual = 5,
        Between = 6,
        NotBetween = 7
    }
}