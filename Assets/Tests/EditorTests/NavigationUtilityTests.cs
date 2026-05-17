using GridLock.Core;
using GridLock.Simulation;
using NUnit.Framework;

namespace GridLock.Tests
{
    public class NavigationUtilityTests
    {
        [TestCase(Direction.North, Direction.South)]
        [TestCase(Direction.South, Direction.North)]
        [TestCase(Direction.East, Direction.West)]
        [TestCase(Direction.West, Direction.East)]
        public void GetOpposite_ReturnsCorrectOpposite(Direction input, Direction expected)
        {
            Assert.AreEqual(expected, NavigationUtility.GetOpposite(input));
        }

        [TestCase(Direction.South, TurnDirection.Straight, Direction.North)]
        [TestCase(Direction.South, TurnDirection.Left, Direction.West)]
        [TestCase(Direction.South, TurnDirection.Right, Direction.East)]
        [TestCase(Direction.North, TurnDirection.Straight, Direction.South)]
        [TestCase(Direction.North, TurnDirection.Left, Direction.East)]
        [TestCase(Direction.North, TurnDirection.Right, Direction.West)]
        [TestCase(Direction.East, TurnDirection.Straight, Direction.West)]
        [TestCase(Direction.East, TurnDirection.Left, Direction.South)]
        [TestCase(Direction.East, TurnDirection.Right, Direction.North)]
        [TestCase(Direction.West, TurnDirection.Straight, Direction.East)]
        [TestCase(Direction.West, TurnDirection.Left, Direction.North)]
        [TestCase(Direction.West, TurnDirection.Right, Direction.South)]
        public void ResolveExitDirection_ReturnsCorrectDirection(Direction approach, TurnDirection turn, Direction expected)
        {
            Assert.AreEqual(expected, NavigationUtility.ResolveExitDirection(approach, turn));
        }

        [TestCase(Direction.South, Direction.North, TurnDirection.Straight)]
        [TestCase(Direction.South, Direction.East, TurnDirection.Right)]
        [TestCase(Direction.South, Direction.West, TurnDirection.Left)]
        [TestCase(Direction.North, Direction.South, TurnDirection.Straight)]
        [TestCase(Direction.North, Direction.West, TurnDirection.Right)]
        [TestCase(Direction.East, Direction.West, TurnDirection.Straight)]
        public void DeduceTurn_ReturnsCorrectTurn(Direction approach, Direction exit, TurnDirection expected)
        {
            Assert.AreEqual(expected, NavigationUtility.DeduceTurn(approach, exit));
        }

        [Test]
        public void AreTurnsConflicting_SameApproach_ReturnsFalse()
        {
            Assert.IsFalse(NavigationUtility.AreTurnsConflicting(
                Direction.South, TurnDirection.Straight,
                Direction.South, TurnDirection.Right));
        }

        [Test]
        public void AreTurnsConflicting_Opposite_BothStraight_ReturnsFalse()
        {
            Assert.IsFalse(NavigationUtility.AreTurnsConflicting(
                Direction.South, TurnDirection.Straight,
                Direction.North, TurnDirection.Straight));
        }

        [Test]
        public void AreTurnsConflicting_Opposite_OneRightTurn_ReturnsTrue()
        {
            Assert.IsTrue(NavigationUtility.AreTurnsConflicting(
                Direction.South, TurnDirection.Right,
                Direction.North, TurnDirection.Straight));
        }

        [Test]
        public void AreTurnsConflicting_Opposite_BothLeft_ReturnsFalse()
        {
            Assert.IsFalse(NavigationUtility.AreTurnsConflicting(
                Direction.South, TurnDirection.Left,
                Direction.North, TurnDirection.Left));
        }

        [Test]
        public void AreTurnsConflicting_Perpendicular_InnerTurnsLeft_ReturnsFalse()
        {
            // South approach, left turn exits West — which is the approach direction of the other vehicle
            // So South is the "inner" vehicle when other approaches from West
            Assert.IsFalse(NavigationUtility.AreTurnsConflicting(
                Direction.South, TurnDirection.Left,
                Direction.West, TurnDirection.Straight));
        }

        [Test]
        public void AreTurnsConflicting_Perpendicular_BothStraight_ReturnsTrue()
        {
            Assert.IsTrue(NavigationUtility.AreTurnsConflicting(
                Direction.South, TurnDirection.Straight,
                Direction.West, TurnDirection.Straight));
        }

        [Test]
        public void AreTurnsConflicting_Perpendicular_OuterTurnsLeft_ReturnsTrue()
        {
            // West approach, left turn exits North — not the approach direction of South
            // So West is NOT the inner vehicle; its left turn still conflicts
            Assert.IsTrue(NavigationUtility.AreTurnsConflicting(
                Direction.South, TurnDirection.Straight,
                Direction.West, TurnDirection.Left));
        }
    }
}
