namespace GridLock.Core
{
    public interface IVehicleMovementConfig
    {
        float Speed { get; }
        float LeftTurnDistance { get; }
        float RightTurnDistance { get; }
        float StraightDistance { get; }
    }
}
