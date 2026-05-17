using System.Collections.Generic;
using GridLock.Core;
using GridLock.Simulation;
using NUnit.Framework;

namespace GridLock.Tests
{
    public class RoadSegmentTests
    {
        private RoadSegment _segment;

        [SetUp]
        public void SetUp()
        {
            _segment = new RoadSegment(1, 3, Direction.North);
        }

        [Test]
        public void HasSpace_ReturnsTrueWhenEmpty()
        {
            Assert.IsTrue(_segment.HasSpace(1));
        }

        [Test]
        public void HasSpace_ReturnsFalseWhenFull()
        {
            var route = CreateMinimalRoute();
            for (int i = 0; i < 3; i++)
            {
                var vehicle = TestUtility.CreateVehicle(route, TestUtility.CreateVehicleConfig(size: 1));
                _segment.AddVehicle(vehicle);
            }

            Assert.IsFalse(_segment.HasSpace(1));
        }

        [Test]
        public void AddAndRemoveVehicle_UpdatesOccupancy()
        {
            var route = CreateMinimalRoute();
            var vehicle = TestUtility.CreateVehicle(route, TestUtility.CreateVehicleConfig(size: 2));

            _segment.AddVehicle(vehicle);
            Assert.IsTrue(_segment.HasSpace(1));
            Assert.IsFalse(_segment.HasSpace(2));

            _segment.RemoveVehicle(vehicle);
            Assert.IsTrue(_segment.HasSpace(2));
        }

        [Test]
        public void ReserveSpace_ReducesAvailable()
        {
            _segment.ReserveSpace(2);

            Assert.IsTrue(_segment.HasSpace(1));
            Assert.IsFalse(_segment.HasSpace(2));
        }

        [Test]
        public void GetMaxVehicleProgress_SingleVehicle_ReturnsOne()
        {
            var route = CreateMinimalRoute();
            var vehicle = TestUtility.CreateVehicle(route, TestUtility.CreateVehicleConfig(size: 1));
            _segment.AddVehicle(vehicle);

            float maxProgress = _segment.GetMaxVehicleProgress(vehicle);

            Assert.AreEqual(1f, maxProgress, 0.001f);
        }

        [Test]
        public void GetMaxVehicleProgress_MultipleVehicles_SecondCappedByFirst()
        {
            var route = CreateMinimalRoute();
            var first = TestUtility.CreateVehicle(route, TestUtility.CreateVehicleConfig(size: 1));
            var second = TestUtility.CreateVehicle(route, TestUtility.CreateVehicleConfig(size: 1));

            _segment.AddVehicle(first);
            _segment.AddVehicle(second);

            // First vehicle (index 0) has no one ahead, max = (3 - 0) / 3 = 1.0
            // Second vehicle (index 1) is behind first (size 1), max = (3 - 1) / 3 = 0.667
            float maxFirst = _segment.GetMaxVehicleProgress(first);
            float maxSecond = _segment.GetMaxVehicleProgress(second);

            Assert.AreEqual(1f, maxFirst, 0.001f);
            Assert.AreEqual(2f / 3f, maxSecond, 0.001f);
        }

        private List<RouteStep> CreateMinimalRoute()
        {
            return new List<RouteStep>
            {
                new RouteStep(_segment, null, Direction.None, Direction.North, TurnDirection.Straight),
            };
        }
    }
}
