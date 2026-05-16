using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GridLock.Config
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/Level Data")]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private float _timeLimit;
        [SerializeField] private float _levelStartSpawnInterval;
        [SerializeField] private float _levelEndSpawnIntervalSpawnInterval;
        [SerializeField] private List<VehicleSpawnEntry> _vehicleSpawnEntries;

        public float TimeLimit => _timeLimit; // in seconds
        public List<VehicleSpawnEntry> VehicleSpawnEntries => _vehicleSpawnEntries;

        public float GetSpawnIntervalForTime(float time)
        {
            return Mathf.Lerp(_levelStartSpawnInterval, _levelEndSpawnIntervalSpawnInterval, time / _timeLimit);
        }

        public VehicleConfig GetRandomVehicleForTime(float time)
        {
            Debug.Assert(_vehicleSpawnEntries.Count > 0, "LevelData has no vehicle spawn entries configured");

            float normalizedTime = Mathf.Clamp01(time / _timeLimit);
            float totalWeight = 0f;

            for (int i = 0; i < _vehicleSpawnEntries.Count; i++)
            {
                totalWeight += _vehicleSpawnEntries[i].SpawnWeightOverTime.Evaluate(normalizedTime);
            }

            float randomNum = Random.value * totalWeight;
            float cumulative = 0f;

            for (int i = 0; i < _vehicleSpawnEntries.Count; i++)
            {
                cumulative += _vehicleSpawnEntries[i].SpawnWeightOverTime.Evaluate(normalizedTime);
                if (randomNum <= cumulative)
                {
                    return _vehicleSpawnEntries[i].VehicleConfig;
                }
            }

            Debug.LogWarning($"Check vehicle generation: Random value - {randomNum}, greater than total weight - {cumulative}");
            return _vehicleSpawnEntries[^1].VehicleConfig;
        }

        [Serializable]
        public class VehicleSpawnEntry
        {
            public VehicleConfig VehicleConfig;
            public AnimationCurve SpawnWeightOverTime;
        }
    }
}
