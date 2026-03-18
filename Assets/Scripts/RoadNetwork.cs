using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The complete road network graph for a level.
/// Built from NetworkLayoutData and provides lookup for CrossRoads and segments.
/// </summary>
public class RoadNetwork
{
    private readonly Dictionary<int, CrossRoads> _crossRoads = new Dictionary<int, CrossRoads>();
    private readonly Dictionary<int, RoadSegment> _segments = new Dictionary<int, RoadSegment>();
    private readonly List<RoadSegment> _spawnSegments = new List<RoadSegment>();
    private readonly List<RoadSegment> _exitSegments = new List<RoadSegment>();
    
    public Dictionary<int, CrossRoads> CrossRoads => _crossRoads;
    public Dictionary<int, RoadSegment> Segments => _segments;
    public List<RoadSegment> SpawnSegments => _spawnSegments;
    public List<RoadSegment> ExitSegments => _exitSegments;

    public void Build(NetworkLayoutData networkLayoutData)
    {
        _crossRoads.Clear();
        _segments.Clear();
        _spawnSegments.Clear();
        _exitSegments.Clear();

        // Create CrossRoads
        foreach (var data in networkLayoutData.CrossRoads)
        {
            var crossRoads = new CrossRoads(data.Id, data.InitialTrafficLightState);
            _crossRoads[data.Id] = crossRoads;
        }

        // Create road segments and wire to CrossRoads
        foreach (var data in networkLayoutData.RoadSegments)
        {
            var segment = new RoadSegment(data.Id, data.Capacity, data.Direction);
            _segments[data.Id] = segment;

            if (data.IsSpawnPoint)
            {
                _spawnSegments.Add(segment);
            }

            if (data.IsExitPoint)
            {
                _exitSegments.Add(segment);
            }

            if (_crossRoads.TryGetValue(data.FromCrossRoadsId, out var fromCrossRoads))
            {
                segment.FromCrossRoads = fromCrossRoads;
                fromCrossRoads.OutboundRoads[data.Direction] = segment;
            }

            if (_crossRoads.TryGetValue(data.ToCrossRoadsId, out var toCrossRoads))
            {
                segment.ToCrossRoads = toCrossRoads;
                toCrossRoads.InboundRoads[data.Direction] = segment;
            }
        }
    }

    public RoadSegment GetSegment(int id)
    {
        _segments.TryGetValue(id, out var segment);
        return segment;
    }
}

public class NetworkLayoutData
{
    public List<CrossRoadsData> CrossRoads = new List<CrossRoadsData>();
    public List<RoadSegmentData> RoadSegments = new List<RoadSegmentData>();
}
