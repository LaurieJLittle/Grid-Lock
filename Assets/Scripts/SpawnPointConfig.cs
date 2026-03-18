using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SpawnPointConfig : MonoBehaviour
{
    [SerializeField] private string _roadSegmentId;

    public SpawnPointData GetSpawnPointData()
    {
        return new SpawnPointData
        {
            RoadSegmentId = _roadSegmentId.GetHashCode()
        };
    }
}

public struct SpawnPointData
{
    public int RoadSegmentId;
}
