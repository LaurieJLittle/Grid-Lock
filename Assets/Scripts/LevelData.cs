using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/Level Data")]
public class LevelData : ScriptableObject
{
    [SerializeField] private float _timeLimit;
    [SerializeField] private float _levelStartSpawnInterval;
    [SerializeField] private float _levelEndSpawnIntervalSpawnInterval;
    [SerializeField] private float _levelStartLorrySpawnProbability;
    [SerializeField] private float _levelEndLorrySpawnProbability;
    
    public float TimeLimit => _timeLimit; // in seconds

    public float GetSpawnIntervalForTime(float time)
    {
        return Mathf.Lerp(_levelStartSpawnInterval, _levelEndSpawnIntervalSpawnInterval, time / _timeLimit);
    }
    
    public float GetLorrySpawnProbabilityForTime(float time)
    {
        return Mathf.Lerp(_levelStartLorrySpawnProbability, _levelEndLorrySpawnProbability, time / _timeLimit);
    }
}
