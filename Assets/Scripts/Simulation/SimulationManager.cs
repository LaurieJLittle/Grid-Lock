using System;
using System.Collections.Generic;
using GridLock.Core;
using UnityEngine;

namespace GridLock.Simulation
{
    public class SimulationManager
    {
        private readonly List<Vehicle> _activeVehicles = new List<Vehicle>();
        private readonly List<Vehicle> _vehiclesToRemove = new List<Vehicle>();
        private readonly Dictionary<int, Dictionary<int, CrossRoadsEntryRequest>> _pendingEntries = new();
        private readonly Dictionary<int, List<int>> _pendingEntryIds = new Dictionary<int, List<int>>();
        private readonly CrossRoadsPrioritization _crossRoadsPrioritization;
        private readonly HashSet<int> _processedRequestIds = new HashSet<int>();
        private float _spawnTimer;
        
        public event Action<Vehicle> OnVehicleSpawned;
        public event Action<Vehicle> OnVehicleDestroyed;

        public SimulationManager(CrossRoadsPrioritization crossRoadsPrioritization)
        {
            _crossRoadsPrioritization = crossRoadsPrioritization;
        }

        public void AddVehicle(Vehicle vehicle, RoadSegment startSegment)
        {
            if (!startSegment.HasSpace(vehicle.VehicleConfig.Size))
            {
                Debug.LogWarning($"Cannot spawn vehicle {vehicle.Id}: no space on segment {startSegment.Id}");
                return;
            }
            
            startSegment.AddVehicle(vehicle);
            vehicle.SetSegment(startSegment);
            _activeVehicles.Add(vehicle);
            OnVehicleSpawned?.Invoke(vehicle);
        }
        
        public void UpdateSimulation(float dt)
        {
            ProcessCrossRoadsTraversals(dt);
            ProcessRoadMovement(dt);
            ProcessCrossRoadsEntries();
            CleanupExitedVehicles();
        }

        private void ProcessCrossRoadsTraversals(float dt)
        {
            foreach (var vehicle in _activeVehicles)
            {
                if (vehicle.State != VehicleState.TraversingCrossRoads)
                {
                    continue;
                }

                float progressDelta = (vehicle.MovementConfig.Speed * dt) / vehicle.CurrentDistance;
                vehicle.Progress = Mathf.Min(vehicle.Progress + progressDelta, 1f);

                if (vehicle.Progress >= 1f)
                {
                    CompleteTraversal(vehicle);
                }
            }
        }

        private void ProcessRoadMovement(float dt)
        {
            foreach (var vehicle in _activeVehicles)
            {
                if (vehicle.State == VehicleState.TraversingCrossRoads || vehicle.State == VehicleState.ExitedNetwork)
                {
                    continue;
                }

                if (vehicle.CurrentSegment == null)
                {
                    continue;
                }
                
                // Advance along road segment
                float progressDelta = (vehicle.MovementConfig.Speed * dt) / vehicle.CurrentDistance;

                float maxProgress = vehicle.CurrentSegment.GetMaxVehicleProgress(vehicle);
                vehicle.Progress = Mathf.Min(vehicle.Progress + progressDelta, maxProgress);

                // Try enter crossroads at end of roadSegment
                if (vehicle.Progress >= 1f)
                {
                    vehicle.Progress = 1f;
                    RequestCrossRoadsEntry(vehicle);
                }
            }
        }

        private void RequestCrossRoadsEntry(Vehicle vehicle)
        {
            RouteStep nextStep = vehicle.GetNextStep();

            // If no next step, vehicle has reached exit
            if (nextStep == null)
            {
                DespawnVehicle(vehicle);
                return;
            }

            CrossRoads crossRoads = vehicle.CurrentSegment.ToCrossRoads;

            if (!_pendingEntries.ContainsKey(crossRoads.Id))
            {
                _pendingEntries.Add(crossRoads.Id, new Dictionary<int, CrossRoadsEntryRequest>());
                _pendingEntryIds.Add(crossRoads.Id, new List<int>());
            }
            
            if (!_pendingEntries[crossRoads.Id].ContainsKey(vehicle.Id))
            {
                _pendingEntries[crossRoads.Id].Add(vehicle.Id, new CrossRoadsEntryRequest
                {
                    Vehicle = vehicle,
                    CrossRoads = crossRoads,
                    ApproachFrom = nextStep.ApproachDirection,
                    Turn = nextStep.Turn,
                    ExitDir = nextStep.ExitDirection,
                });
                
                _pendingEntryIds[crossRoads.Id].Add(vehicle.Id);
            }

            vehicle.SetState(VehicleState.Queued);
        }

        private void ProcessCrossRoadsEntries()
        {
            foreach (var crossRoadsId in _pendingEntries.Keys)
            {
                _processedRequestIds.Clear();
                foreach (var vehicleId in _pendingEntryIds[crossRoadsId])
                {
                    var req = _pendingEntries[crossRoadsId][vehicleId];

                    if (!req.CrossRoads.CanEnterCrossRoads(req.ApproachFrom, req.Turn))
                    {
                        continue;
                    }

                    if (!req.CrossRoads.OutboundRoadHasExitSpace(req.ApproachFrom, req.Turn, req.Vehicle.VehicleConfig.Size, out RoadSegment outboundRoad))
                    {
                        if (_crossRoadsPrioritization == CrossRoadsPrioritization.FreeForAll)
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (req.CrossRoads.IsPathConflicting(req.ApproachFrom, req.Turn))
                    {
                        if (_crossRoadsPrioritization == CrossRoadsPrioritization.FreeForAll)
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // Entry granted
                    outboundRoad.ReserveSpace(req.Vehicle.VehicleConfig.Size);
                    req.CrossRoads.ReservePath(req.Vehicle, req.ApproachFrom, req.Turn);
                    req.Vehicle.CurrentSegment?.RemoveVehicle(req.Vehicle);
                    req.Vehicle.BeginTraversal(req.CrossRoads, req.Turn);
                    _processedRequestIds.Add(vehicleId);
                }

                foreach (var requestId in _processedRequestIds)
                {
                    _pendingEntries[crossRoadsId].Remove(requestId);
                    _pendingEntryIds[crossRoadsId].Remove(requestId);
                }
            }
        }

        private void CompleteTraversal(Vehicle vehicle)
        {
            CrossRoads CrossRoads = vehicle.TraversingCrossRoads;
            CrossRoads?.ReleasePath(vehicle);

            vehicle.AdvanceRoute();
            TransferToNextSegment(vehicle);
        }

        private void TransferToNextSegment(Vehicle vehicle)
        {
            RouteStep step = vehicle.GetCurrentStep();

            step.Segment.AddVehicle(vehicle, freeReservedSpace: true);
            vehicle.SetSegment(step.Segment);
            vehicle.SetState(VehicleState.TraversingRoadSegment);
        }

        private void DespawnVehicle(Vehicle vehicle)
        {
            vehicle.CurrentSegment?.RemoveVehicle(vehicle);
            vehicle.MarkExited(Time.time);
            _vehiclesToRemove.Add(vehicle);
        }
        
        private void CleanupExitedVehicles()
        {
            foreach (var vehicle in _vehiclesToRemove)
            {
                _activeVehicles.Remove(vehicle);
                OnVehicleDestroyed?.Invoke(vehicle);
            }
            _vehiclesToRemove.Clear();
        }

        private struct CrossRoadsEntryRequest
        {
            public Vehicle Vehicle;
            public CrossRoads CrossRoads;
            public Direction ApproachFrom;
            public Direction ExitDir;
            public TurnDirection Turn;
        }
    }
}
