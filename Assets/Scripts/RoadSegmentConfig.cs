using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class RoadSegmentConfig : MonoBehaviour
{
    [SerializeField] private string _id;
    [SerializeField] private string _fromCrossRoadsId;
    [SerializeField] private string _toCrossRoadsId;
    [SerializeField] private Direction _direction;
    [SerializeField] private int _capacity;
    [SerializeField] private bool _isSpawnPoint;
    [SerializeField] private bool _isExitPoint;

    public string Id => _id;

    public RoadSegmentData GetRoadSegmentData()
    {
        return new RoadSegmentData
        {
            Id = _id.GetHashCode(),
            FromCrossRoadsId = _fromCrossRoadsId.GetHashCode(),
            ToCrossRoadsId = _toCrossRoadsId.GetHashCode(),
            Direction = _direction,
            Capacity = _capacity,
            IsSpawnPoint = _isSpawnPoint,
            IsExitPoint = _isExitPoint,
        };
    }
}

public struct RoadSegmentData
{
    public int Id;
    public int FromCrossRoadsId;
    public int ToCrossRoadsId;
    public Direction Direction;
    public int Capacity;
    public bool IsSpawnPoint;
    public bool IsExitPoint;
}