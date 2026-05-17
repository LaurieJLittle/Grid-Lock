using System.Collections.Generic;
using GridLock.Config;
using GridLock.Core;
using GridLock.Simulation;
using UnityEngine;

namespace GridLock.View
{
    public class RoundManager : RoundManagerBase
    {
        [SerializeField] private CrossRoadsConfig[] _crossRoads;
        [SerializeField] private RoadSegmentConfig[] _roadSegments;
        [SerializeField] private SpawnPointConfig[] _spawnPoints;
        [SerializeField] private ExitPointConfig[] _exitPoints;

        protected override RoadNetwork BuildNetwork()
        {
            var crossRoadsData = new List<CrossRoadsData>();
            foreach (var crossRoadsConfig in _crossRoads)
            {
                crossRoadsData.Add(crossRoadsConfig.GetCrossRoadsData());
            }

            var roadSegmentsData = new List<RoadSegmentData>();
            foreach (var roadSegmentConfig in _roadSegments)
            {
                roadSegmentsData.Add(roadSegmentConfig.GetRoadSegmentData());
            }

            var networkLayoutData = new NetworkLayoutData
            {
                CrossRoads = crossRoadsData,
                RoadSegments = roadSegmentsData,
            };

            var network = new RoadNetwork();
            network.Build(networkLayoutData);

            foreach (var spawnPoint in _spawnPoints)
            {
                network.RegisterSpawnPoint(spawnPoint.RoadSegment.Id.GetHashCode());
            }

            foreach (var exitPoint in _exitPoints)
            {
                network.RegisterExitPoint(exitPoint.RoadSegment.Id.GetHashCode(), exitPoint.VehicleId);
            }

            return network;
        }
    }
}
