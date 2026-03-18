using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class CrossRoadsConfig : MonoBehaviour
{
    [SerializeField] private string _id;
    [SerializeField] private TrafficLightState _initialTrafficLightState;

    public string Id => _id;

    public CrossRoadsData GetCrossRoadsData()
    {
        return new CrossRoadsData
        {
            Id = _id.GetHashCode(),
            InitialTrafficLightState = _initialTrafficLightState,
        };
    }
}

public struct CrossRoadsData
{
    public int Id;
    public TrafficLightState InitialTrafficLightState;
}