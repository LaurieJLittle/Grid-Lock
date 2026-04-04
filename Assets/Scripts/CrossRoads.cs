using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossRoads
{
    private readonly Dictionary<int, TraversalInfo> _traversalDetails = new Dictionary<int, TraversalInfo>();
    
    public int Id { get; }
    public TrafficLightState CurrentLight { get; private set; }

    // Inbound road segments keyed by the direction they head towards (not arriving from) i.e, a north bound road arrives at the south of the junction but its key is still north as it is heading north
    public Dictionary<Direction, RoadSegment> InboundRoads { get; } = new Dictionary<Direction, RoadSegment>();
    // Outbound road segments keyed by the direction they head towards.
    public Dictionary<Direction, RoadSegment> OutboundRoads { get; } = new Dictionary<Direction, RoadSegment>();

    public CrossRoads(int id, TrafficLightState initialState)
    {
        Id = id;
        CurrentLight = initialState;
    }

    public void ToggleTrafficLights()
    {
        if (CurrentLight == TrafficLightState.NorthSouth)
        {
            CurrentLight = TrafficLightState.EastWest;
        }
        else
        {
            CurrentLight = TrafficLightState.NorthSouth;
        }
    }

    public bool CanEnterCrossRoads(Direction approachFrom, TurnDirection turn)
    {
        if (!TrafficLightsPermitEntranceFromDirection(approachFrom))
        {
            return false;
        }

        RoadSegment exitRoad = GetExitRoad(approachFrom, turn);
        if (exitRoad == null)
        {
            Debug.LogError($"Car trying to exit crossroads {Id} in invalid direction");
            return false;
        }

        return true;
    }

    public bool OutboundRoadHasExitSpace(Direction approachFrom, TurnDirection turn, int vehicleSize, out RoadSegment outboundRoad)
    {
        outboundRoad = GetExitRoad(approachFrom, turn);
        return outboundRoad != null && outboundRoad.HasSpace(vehicleSize);
    }
    
    public void ReservePath(Vehicle vehicle, Direction approachFrom, TurnDirection turn)
    {
        var info = new TraversalInfo(approachFrom, turn);
        _traversalDetails[vehicle.Id] = info;
    }

    public void ReleasePath(Vehicle vehicle)
    {
        _traversalDetails.Remove(vehicle.Id);
    }

    public bool IsPathConflicting(Direction approachFrom, TurnDirection turn)
    {
        foreach (var kvp in _traversalDetails)
        {
            if (NavigationUtility.AreTurnsConflicting(
                    kvp.Value.ApproachFrom, kvp.Value.Turn, approachFrom, turn))
            {
                return true;
            }
        }
        return false;
    }

    private bool TrafficLightsPermitEntranceFromDirection(Direction approachFrom)
    {
        switch (CurrentLight)
        {
            case TrafficLightState.NorthSouth:
                return approachFrom == Direction.North || approachFrom == Direction.South;
            case TrafficLightState.EastWest:
                return approachFrom == Direction.East || approachFrom == Direction.West;
        }

        Debug.LogError($"Error checking if traffic can enter crossroads, not handling Traffic light state {CurrentLight}");
        return false;
    }

    private RoadSegment GetExitRoad(Direction approachFrom, TurnDirection turn)
    {
        Direction exitDir = NavigationUtility.ResolveExitDirection(approachFrom, turn);
        OutboundRoads.TryGetValue(exitDir, out RoadSegment road);
        return road;
    }
    
    private readonly struct TraversalInfo
    {
        public readonly Direction ApproachFrom;
        public readonly TurnDirection Turn;

        public TraversalInfo(Direction approachFrom, TurnDirection turn)
        {
            ApproachFrom = approachFrom;
            Turn = turn;
        }
    }
}
