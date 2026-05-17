using System;
using System.Collections.Generic;
using GridLock.Config;
using GridLock.Core;

namespace GridLock.Simulation
{
    public class Vehicle
    {
        private static int _nextId;
        private readonly List<RouteStep> _route;

        public IVehicleConfig VehicleConfig { get; }
        public IVehicleMovementConfig MovementConfig { get; }
        public int Id { get; }
        public VehicleState State { get; private set; }
        private int CurrentRouteIndex { get; set; }

        public RoadSegment CurrentSegment { get; private set; } // null if on crossroads
        public CrossRoads TraversingCrossRoads { get; private set; } // null if on road segment

        // Unified traversal progress (0 to 1) and distance for both road segments and crossroads
        public float Progress { get; set; }
        public float CurrentDistance { get; private set; }

        public event Action<Vehicle, VehicleState> OnStateChanged;
        public event Action TripComplete;

        public Vehicle(List<RouteStep> route, IVehicleConfig vehicleConfig, IVehicleMovementConfig movementConfig)
        {
            Id = _nextId++;
            _route = route;
            VehicleConfig = vehicleConfig;
            MovementConfig = movementConfig;
            State = VehicleState.TraversingRoadSegment;
            CurrentRouteIndex = 0;
        }

        public void SetSegment(RoadSegment segment)
        {
            CurrentSegment = segment;
            TraversingCrossRoads = null;
            Progress = 0f;
            CurrentDistance = segment.Capacity;
        }

        public void BeginTraversal(CrossRoads CrossRoads, TurnDirection turnDirection)
        {
            CurrentSegment = null;
            TraversingCrossRoads = CrossRoads;

            switch (turnDirection)
            {
                case TurnDirection.Left:
                    CurrentDistance = MovementConfig.LeftTurnDistance;
                    break;
                case TurnDirection.Right:
                    CurrentDistance = MovementConfig.RightTurnDistance;
                    break;
                case TurnDirection.Straight:
                    CurrentDistance = MovementConfig.StraightDistance;
                    break;
                default:
                    CurrentDistance = MovementConfig.StraightDistance;
                    break;
            }

            Progress = 0f;
            SetState(VehicleState.TraversingCrossRoads);
        }

        public void AdvanceRoute()
        {
            CurrentRouteIndex++;
        }

        public RouteStep GetCurrentStep()
        {
            if (CurrentRouteIndex < _route.Count)
            {
                return _route[CurrentRouteIndex];
            }

            return null;
        }

        public RouteStep GetNextStep()
        {
            int next = CurrentRouteIndex + 1;
            if (next < _route.Count)
            {
                return _route[next];
            }

            return null;
        }

        public void SetState(VehicleState newState)
        {
            if (State == newState)
            {
                return;
            }

            State = newState;
            OnStateChanged?.Invoke(this, newState);
        }

        public void MarkExited(float time)
        {
            SetState(VehicleState.ExitedNetwork);
            TripComplete.Invoke();
        }
    }

    public class RouteStep
    {
        public RoadSegment Segment { get; }
        public CrossRoads CrossRoads { get; }
        public TurnDirection Turn { get; }
        public Direction ApproachDirection { get; }
        public Direction ExitDirection { get; }

        public RouteStep(RoadSegment segment, CrossRoads crossRoads, Direction approachDirection, Direction exitDirection, TurnDirection turn)
        {
            Segment = segment;
            CrossRoads = crossRoads;
            ApproachDirection = approachDirection;
            ExitDirection = exitDirection;
            Turn = turn;
        }
    }
}
