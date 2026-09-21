namespace GridLock.Core
{
    public struct RoadSegmentData
    {
        public int Id;
        public int FromCrossRoadsId;
        public int ToCrossRoadsId;
        public Direction Direction;
        public int Capacity;
    }

    public struct CrossRoadsData
    {
        public int Id;
        public TrafficLightState InitialTrafficLightState;
    }
}
