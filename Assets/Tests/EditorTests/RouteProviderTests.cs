using System.Collections.Generic;
using GridLock.Config;
using GridLock.Core;
using GridLock.Simulation;
using NUnit.Framework;

namespace GridLock.Tests
{
    public class RouteProviderTests
    {
        private RouteProvider _routeProvider;

        [SetUp]
        public void SetUp()
        {
            _routeProvider = new RouteProvider();
        }

        [Test]
        public void FindRoute_ReturnsValidRoute()
        {
            var result = TestUtility.BuildSimpleNetwork();

            List<RouteStep> route = _routeProvider.FindRoute(result.SpawnSegment, result.ExitSegment);

            Assert.IsNotNull(route);
            Assert.AreEqual(2, route.Count);
            Assert.AreEqual(result.SpawnSegment, route[0].Segment);
            Assert.AreEqual(result.ExitSegment, route[1].Segment);
        }

        [Test]
        public void FindRoute_ReturnsNull_WhenUnreachable()
        {
            // Create a disconnected segment that isn't wired to any crossroads
            var disconnectedExit = new RoadSegment(999, 3, Direction.East);

            var result = TestUtility.BuildSimpleNetwork();

            List<RouteStep> route = _routeProvider.FindRoute(result.SpawnSegment, disconnectedExit);

            Assert.IsNull(route);
        }

        [Test]
        public void FindRoute_AvoidsUTurns()
        {
            // Build a network with 2 crossroads:
            //   spawnSeg (North) -> CR1 -> midSeg (East) -> CR2 -> exitSeg (East)
            //   CR1 also has a southbound road that leads back (U-turn from spawn approach)
            var network = new RoadNetwork();
            var layoutData = new NetworkLayoutData();

            layoutData.CrossRoads.Add(new CrossRoadsData { Id = 1, InitialTrafficLightState = TrafficLightState.NorthSouth });
            layoutData.CrossRoads.Add(new CrossRoadsData { Id = 2, InitialTrafficLightState = TrafficLightState.EastWest });

            // Spawn segment: heads North into CR1
            layoutData.RoadSegments.Add(new RoadSegmentData
            {
                Id = 10, FromCrossRoadsId = -1, ToCrossRoadsId = 1,
                Direction = Direction.North, Capacity = 3,
            });

            // Mid segment: heads East from CR1 to CR2
            layoutData.RoadSegments.Add(new RoadSegmentData
            {
                Id = 20, FromCrossRoadsId = 1, ToCrossRoadsId = 2,
                Direction = Direction.East, Capacity = 3,
            });

            // U-turn segment: heads South out of CR1 (same direction as approach)
            layoutData.RoadSegments.Add(new RoadSegmentData
            {
                Id = 30, FromCrossRoadsId = 1, ToCrossRoadsId = -1,
                Direction = Direction.South, Capacity = 3,
            });

            // Exit segment: heads East out of CR2
            layoutData.RoadSegments.Add(new RoadSegmentData
            {
                Id = 40, FromCrossRoadsId = 2, ToCrossRoadsId = -1,
                Direction = Direction.East, Capacity = 3,
            });

            network.Build(layoutData);

            var spawnSeg = network.GetSegment(10);
            var exitSeg = network.GetSegment(40);
            var uTurnSeg = network.GetSegment(30);

            List<RouteStep> route = _routeProvider.FindRoute(spawnSeg, exitSeg);

            Assert.IsNotNull(route);
            foreach (var step in route)
            {
                Assert.AreNotEqual(uTurnSeg, step.Segment, "Route should not contain the U-turn segment");
            }
        }
    }
}
