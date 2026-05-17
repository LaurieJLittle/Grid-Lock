using System.Collections.Generic;
using GridLock.Core;
using GridLock.Simulation;
using NUnit.Framework;

namespace GridLock.Tests
{
    public class SimulationManagerTests
    {
        private SimulationManager _sim;
        private TestUtility.SimpleNetworkResult _net;

        [SetUp]
        public void SetUp()
        {
            _sim = new SimulationManager(CrossRoadsPrioritization.FreeForAll);
            _net = TestUtility.BuildSimpleNetwork();
        }

        [Test]
        public void AddVehicle_FiresOnVehicleSpawned()
        {
            var route = BuildSpawnToExitRoute();
            var vehicle = TestUtility.CreateVehicle(route);

            Vehicle spawnedVehicle = null;
            _sim.OnVehicleSpawned += v => spawnedVehicle = v;

            _sim.AddVehicle(vehicle, _net.SpawnSegment);

            Assert.AreEqual(vehicle, spawnedVehicle);
        }

        [Test]
        public void AddVehicle_RejectsWhenNoSpace()
        {
            // Fill the spawn segment (capacity 3) with size-1 vehicles
            for (int i = 0; i < 3; i++)
            {
                var v = TestUtility.CreateVehicle(BuildSpawnToExitRoute());
                _sim.AddVehicle(v, _net.SpawnSegment);
            }

            bool fired = false;
            _sim.OnVehicleSpawned += v => fired = true;

            var rejected = TestUtility.CreateVehicle(BuildSpawnToExitRoute());
            _sim.AddVehicle(rejected, _net.SpawnSegment);

            Assert.IsFalse(fired);
        }

        [Test]
        public void UpdateSimulation_VehicleTraversesRoadSegment()
        {
            var route = BuildSpawnToExitRoute();
            var vehicle = TestUtility.CreateVehicle(route, movementConfig: TestUtility.CreateMovementConfig(speed: 1f));

            _sim.AddVehicle(vehicle, _net.SpawnSegment);

            // Vehicle starts at progress 0, segment capacity is 3
            // progressDelta = (speed * dt) / capacity = (1 * 0.5) / 3 = 0.167
            _sim.UpdateSimulation(0.5f);

            Assert.Greater(vehicle.Progress, 0f);
            Assert.AreEqual(VehicleState.TraversingRoadSegment, vehicle.State);
        }

        [Test]
        public void UpdateSimulation_VehicleExitsNetwork()
        {
            // Create a route with only the spawn segment (no next step)
            // so when the vehicle reaches the end, it exits
            var route = new List<RouteStep>
            {
                new RouteStep(_net.SpawnSegment, null, Direction.None, Direction.North, TurnDirection.Straight),
            };
            var vehicle = TestUtility.CreateVehicle(route, movementConfig: TestUtility.CreateMovementConfig(speed: 100f));
            vehicle.TripComplete += () => { };

            _sim.AddVehicle(vehicle, _net.SpawnSegment);

            // Large dt so vehicle reaches end of segment
            _sim.UpdateSimulation(10f);

            Assert.AreEqual(VehicleState.ExitedNetwork, vehicle.State);
        }

        [Test]
        public void UpdateSimulation_VehicleQueuesAtRedLight()
        {
            // Set traffic light to EastWest so North approach is blocked
            _net.CrossRoads.ToggleTrafficLights();
            Assert.AreEqual(TrafficLightState.EastWest, _net.CrossRoads.CurrentLight);

            var route = BuildSpawnToExitRoute();
            var vehicle = TestUtility.CreateVehicle(route, movementConfig: TestUtility.CreateMovementConfig(speed: 100f));

            _sim.AddVehicle(vehicle, _net.SpawnSegment);

            // Large dt so vehicle reaches crossroads
            _sim.UpdateSimulation(10f);

            Assert.AreEqual(VehicleState.Queued, vehicle.State);
        }

        private List<RouteStep> BuildSpawnToExitRoute()
        {
            return new List<RouteStep>
            {
                new RouteStep(_net.SpawnSegment, null, Direction.None, Direction.North, TurnDirection.Straight),
                new RouteStep(_net.ExitSegment, _net.CrossRoads, Direction.South, Direction.North, TurnDirection.Straight),
            };
        }
    }
}
