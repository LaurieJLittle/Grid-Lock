public enum CrossRoadsPrioritization
{
    LongestWaiterHasPriority, // 'fair' traffic, stops build ups
    FreeForAll, // essentially allows long chains of cars to pass through from same direction and opposing direction has to wait for all clear
}

public enum Direction
{
    None,
    North,
    East,
    South,
    West,
}

public enum TurnDirection
{
    Straight,
    Left,
    Right,
}

public enum TrafficLightState
{
    NorthSouth,
    EastWest,
}
