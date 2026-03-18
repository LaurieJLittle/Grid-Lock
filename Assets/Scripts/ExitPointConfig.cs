using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ExitPointConfig : MonoBehaviour
{
    [SerializeField] private string _roadSegmentId;

    public ExitPointData GetExitPointData()
    {
        return new ExitPointData
        {
            RoadSegmentId = _roadSegmentId.GetHashCode()
        };
    }
}

public struct ExitPointData
{
    public int RoadSegmentId;
}