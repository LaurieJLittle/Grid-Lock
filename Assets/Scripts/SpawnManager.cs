using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager
{
    private const float kSpawnHeadsUp = 1f;

    private readonly RoadNetwork _network;
    private readonly RouteProvider _routeProvider;
    private readonly Queue<PendingSpawn> _pendingSpawns = new Queue<PendingSpawn>();
    private float _nextSpawnCountdown;
    private RoadSegment _lastSpawnSegment;

    public event Action<Vehicle, RoadSegment> OnVehicleReadyToSpawn;
    public event Action<RoadSegment> OnSpawnFailed;

    public SpawnManager(RoadNetwork network)
    {
        _network = network;
        _routeProvider = new RouteProvider();
    }

    public void Update(float dt)
    {
        if (_pendingSpawns.Count == 0)
        {
            return;
        }

        _nextSpawnCountdown -= dt;

        if (_nextSpawnCountdown <= 0f)
        {
            SpawnVehicle(_pendingSpawns.Dequeue());

            if (_pendingSpawns.Count > 0)
            {
                _nextSpawnCountdown += kSpawnHeadsUp;
            }
        }
    }
    
    public void QueueSpawn(VehicleConfig vehicleConfig)
    {
        if (_network.SpawnSegments.Count == 0)
        {
            Debug.LogWarning("Can't spawn vehicle, no spawn points found");
            return;
        }

        RoadSegment spawnSegment = GetNextSpawnSegment();
        _lastSpawnSegment = spawnSegment;
        spawnSegment.MarkSpawnPending(kSpawnHeadsUp, vehicleConfig);

        var pending = new PendingSpawn(vehicleConfig, spawnSegment);
        _pendingSpawns.Enqueue(pending);

        if (_pendingSpawns.Count == 1)
        {
            _nextSpawnCountdown = kSpawnHeadsUp;
        }
    }
    
    private void SpawnVehicle(PendingSpawn pendingSpawn)
    {
        // if there is no space on the road segment, just don't spawn, creates
        // a mechanism where player maximises score by keeping as many of the
        // spawn road segments clear for as much of the playtime as possible 
        if (!pendingSpawn.SpawnSegment.HasSpace(pendingSpawn.VehicleConfig.Size))
        {
            Debug.Log("Spawn segment doesn't have space available");
            OnSpawnFailed?.Invoke(pendingSpawn.SpawnSegment);
            return;
        }

        List<RouteStep> route = _routeProvider.FindRoute(pendingSpawn.SpawnSegment, _network.ExitSegments.RandomItem());
        if (route == null || route.Count == 0)
        {
            Debug.LogWarning($"No valid route from segment {pendingSpawn.SpawnSegment.Id}");
            OnSpawnFailed?.Invoke(pendingSpawn.SpawnSegment);
            return;
        }

        var vehicle = new Vehicle(route, pendingSpawn.VehicleConfig);
        OnVehicleReadyToSpawn?.Invoke(vehicle, pendingSpawn.SpawnSegment);
    }

    private RoadSegment GetNextSpawnSegment()
    {
        var segments = _network.SpawnSegments;
        if (segments.Count <= 1 || _lastSpawnSegment == null)
        {
            return segments.RandomItem();
        }

        RoadSegment potentialSpawnSegment;
        do
        {
            potentialSpawnSegment = segments.RandomItem();
        } while (potentialSpawnSegment == _lastSpawnSegment);

        return potentialSpawnSegment;
    }

    private class PendingSpawn
    {
        public readonly VehicleConfig VehicleConfig;
        public readonly RoadSegment SpawnSegment;

        public PendingSpawn(VehicleConfig vehicleConfig, RoadSegment spawnSegment)
        {
            VehicleConfig = vehicleConfig;
            SpawnSegment = spawnSegment;
        }
    }
}
