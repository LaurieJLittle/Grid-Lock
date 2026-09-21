using System;
using System.Collections.Generic;
using GridLock.Core;

namespace GridLock.Simulation
{
    public class SpawnManager
    {
        private const float kSpawnHeadsUp = 1f;

        private readonly IReadOnlyRoadNetwork _network;
        private readonly IVehicleMovementConfig _movementConfig;
        private readonly IRouteProvider _routeProvider;
        private readonly Queue<PendingSpawn> _pendingSpawns = new Queue<PendingSpawn>();
        private float _nextSpawnCountdown;
        private RoadSegment _lastSpawnSegment;

        public event Action<Vehicle, RoadSegment, IVehicleConfig> OnVehicleReadyToSpawn;
        public event Action<RoadSegment> OnSpawnFailed;

        public SpawnManager(IReadOnlyRoadNetwork network, IVehicleMovementConfig movementConfig, IRouteProvider routeProvider)
        {
            _network = network;
            _movementConfig = movementConfig;
            _routeProvider = routeProvider;
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

        public void QueueSpawn(IVehicleConfig vehicleConfig)
        {
            if (_network.SpawnSegments.Count == 0)
            {
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
                OnSpawnFailed?.Invoke(pendingSpawn.SpawnSegment);
                return;
            }

            if (!_network.ExitSegments.TryGetValue(pendingSpawn.VehicleConfig.Id, out var exitSegment))
            {
                throw new InvalidOperationException($"No exit configured for vehicle type {pendingSpawn.VehicleConfig.Id}");
            }

            List<RouteStep> route = _routeProvider.FindRoute(pendingSpawn.SpawnSegment, exitSegment);
            if (route == null || route.Count == 0)
            {
                OnSpawnFailed?.Invoke(pendingSpawn.SpawnSegment);
                return;
            }

            var vehicle = new Vehicle(route, pendingSpawn.VehicleConfig, _movementConfig);
            OnVehicleReadyToSpawn?.Invoke(vehicle, pendingSpawn.SpawnSegment, pendingSpawn.VehicleConfig);
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
            public readonly IVehicleConfig VehicleConfig;
            public readonly RoadSegment SpawnSegment;

            public PendingSpawn(IVehicleConfig vehicleConfig, RoadSegment spawnSegment)
            {
                VehicleConfig = vehicleConfig;
                SpawnSegment = spawnSegment;
            }
        }
    }
}
