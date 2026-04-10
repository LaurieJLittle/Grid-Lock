using System;
using System.Collections.Generic;
using GridLock.Config;
using UnityEngine;

namespace GridLock.View
{
    public class ScoreParticleFactory : MonoBehaviour
    {
        [SerializeField] private ScoreParticleView _scoreParticlePrefab;
        [SerializeField] private VehicleMaterialEntry[] _materialVariants;

        private Dictionary<VehicleId, Material> _materialDict;

        private void Awake()
        {
            _materialDict = new Dictionary<VehicleId, Material>();
            if (_materialVariants != null)
            {
                foreach (VehicleMaterialEntry entry in _materialVariants)
                {
                    if (entry.material != null)
                    {
                        _materialDict[entry.vehicleId] = entry.material;
                    }
                }
            }
        }

        public void Spawn(Vector3 start, Vector3 end, VehicleId vehicleId, Action onArrive)
        {
            if (_scoreParticlePrefab == null)
            {
                return;
            }

            _materialDict.TryGetValue(vehicleId, out Material material);
            ScoreParticleView particle = Instantiate(_scoreParticlePrefab);
            particle.MoveToScoreUI(start, end, material, onArrive);
        }

        [Serializable]
        private class VehicleMaterialEntry
        {
            public VehicleId vehicleId;
            public Material material;
        }
    }
}
