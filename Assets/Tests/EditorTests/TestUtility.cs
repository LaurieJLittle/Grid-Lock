using System.Collections.Generic;
using GridLock.Config;
using GridLock.Core;
using GridLock.Simulation;

namespace GridLock.Tests
{
    public static class TestUtility
    {
        public static IVehicleConfig CreateVehicleConfig(VehicleId id = VehicleId.BlueCar, int size = 1)
        {
            return new TestVehicleConfig(id, size);
        }

        public static IVehicleMovementConfig CreateMovementConfig(
            float speed = 2f,
            float straightDist = 3.75f,
            float leftDist = 3f,
            float rightDist = 4.5f)
        {
            return new TestVehicleMovementConfig(speed, straightDist, leftDist, rightDist);
        }

        public static SimpleNetworkResult BuildSimpleNetwork()
        {
            // Layout: spawnSegment -> crossroads -> exitSegment
            // Spawn segment heads North into crossroads from the South
            // Exit segment heads North out of crossroads
            var network = new RoadNetwork();
            var layoutData = new NetworkLayoutData();

            layoutData.CrossRoads.Add(new CrossRoadsData
            {
                Id = 1,
                InitialTrafficLightState = TrafficLightState.NorthSouth,
            });

            layoutData.RoadSegments.Add(new RoadSegmentData
            {
                Id = 10,
                FromCrossRoadsId = -1,
                ToCrossRoadsId = 1,
                Direction = Direction.North,
                Capacity = 3,
            });

            layoutData.RoadSegments.Add(new RoadSegmentData
            {
                Id = 20,
                FromCrossRoadsId = 1,
                ToCrossRoadsId = -1,
                Direction = Direction.North,
                Capacity = 3,
            });

            network.Build(layoutData);
            network.RegisterSpawnPoint(10);
            network.RegisterExitPointForAllVehicles(20);

            return new SimpleNetworkResult
            {
                Network = network,
                SpawnSegment = network.GetSegment(10),
                ExitSegment = network.GetSegment(20),
                CrossRoads = network.CrossRoads[1],
            };
        }

        public static Vehicle CreateVehicle(List<RouteStep> route, IVehicleConfig vehicleConfig = null, IVehicleMovementConfig movementConfig = null)
        {
            vehicleConfig = vehicleConfig ?? CreateVehicleConfig();
            movementConfig = movementConfig ?? CreateMovementConfig();
            return new Vehicle(route, vehicleConfig, movementConfig);
        }

        public struct SimpleNetworkResult
        {
            public RoadNetwork Network;
            public RoadSegment SpawnSegment;
            public RoadSegment ExitSegment;
            public CrossRoads CrossRoads;
        }

        private class TestVehicleConfig : IVehicleConfig
        {
            public VehicleId Id { get; }
            public int Size { get; }

            public TestVehicleConfig(VehicleId id, int size)
            {
                Id = id;
                Size = size;
            }
        }

        private class TestVehicleMovementConfig : IVehicleMovementConfig
        {
            public float Speed { get; }
            public float LeftTurnDistance { get; }
            public float RightTurnDistance { get; }
            public float StraightDistance { get; }

            public TestVehicleMovementConfig(float speed, float straightDist, float leftDist, float rightDist)
            {
                Speed = speed;
                StraightDistance = straightDist;
                LeftTurnDistance = leftDist;
                RightTurnDistance = rightDist;
            }
        }
    }
}
