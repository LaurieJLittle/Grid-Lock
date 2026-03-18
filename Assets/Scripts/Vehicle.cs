using System;
using System.Collections.Generic;
using UnityEngine;


public enum VehicleState
{
    TraversingRoadSegment,
    Queued,
    TraversingCrossRoads,
    ExitedNetwork
}

public class Vehicle
{
    private static int _nextId;
    private readonly List<RouteStep> _route;
    
    public VehicleConfig VehicleConfig { get; }
    public int Id { get; }
    public VehicleState State { get; private set; }
    private int CurrentRouteIndex { get; set; }

    // Properties used when traversing RoadSegments
    public RoadSegment CurrentSegment { get; private set; } // null if on crossroads
    // From 0 to 1
    public float SegmentProgress { get; set; }
    

    // Properties used when traversing CrossRoads
    public CrossRoads TraversingCrossRoads { get; private set; } // null if on road segment
    public float FullTraversalTime { get; private set; }
    public float CrossroadsTraversalTimeLeft { get; set; }

    public event Action<Vehicle, VehicleState> OnStateChanged;
    public event Action TripComplete;

    public Vehicle(List<RouteStep> route, VehicleConfig vehicleConfig)
    {
        Id = _nextId++;
        _route = route;
        VehicleConfig = vehicleConfig;
        State = VehicleState.TraversingRoadSegment;
        CurrentRouteIndex = 0;
    }

    public void SetSegment(RoadSegment segment)
    {
        CurrentSegment = segment;
        TraversingCrossRoads = null;
        SegmentProgress = 0f;
    }

    public void BeginTraversal(CrossRoads CrossRoads, TurnDirection turnDirection)
    {
        CurrentSegment = null;
        TraversingCrossRoads = CrossRoads;
        
        switch (turnDirection)
        {
            case TurnDirection.Left:
                FullTraversalTime = VehicleConfig.LeftTurnTraversalTime;
                break;
            case TurnDirection.Right:
                FullTraversalTime = VehicleConfig.RightTurnTraversalTime;
                break;
            case TurnDirection.Straight:
                FullTraversalTime = VehicleConfig.StraightTraversalTime;
                break;
        }
        
        CrossroadsTraversalTimeLeft = FullTraversalTime;
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
