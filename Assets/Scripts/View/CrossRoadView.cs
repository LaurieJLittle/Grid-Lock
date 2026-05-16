using GridLock.Config;
using GridLock.Core;
using GridLock.Simulation;
using UnityEngine;

namespace GridLock.View
{
    public class CrossRoadView : MonoBehaviour
    {
        [SerializeField] private CrossRoadsConfig _crossRoadsConfig;
        [SerializeField] private SpriteRenderer _bg; 
        [SerializeField] private Color _indicatorActiveColor;
        [SerializeField] private Color _indicatorInactiveColor; 
        [SerializeField] private SpriteRenderer[] _eastWestIndicators;
        [SerializeField] private SpriteRenderer[] _northSouthIndicators;
        
        private CrossRoads _crossRoads;
        private int _overrideId = int.MinValue;

        public int Id => _overrideId != int.MinValue ? _overrideId : _crossRoadsConfig.Id.GetHashCode();

        public void SetIdOverride(int id)
        {
            _overrideId = id;
        }
        
        public void BindToNetwork(CrossRoads crossRoads)
        {
            _crossRoads = crossRoads;
            UpdateVisuals();
        }

        private void Update()
        {
            float colorVariation = 0.95f + (0.05f * Mathf.Cos(2f * Time.time));
            _bg.color = new Color(colorVariation, colorVariation, colorVariation);
        }

        public void ToggleLight()
        {
            _crossRoads.ToggleTrafficLights();
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            foreach (var indicator in _eastWestIndicators)
            {
                indicator.color = _crossRoads.CurrentLight == TrafficLightState.EastWest ? _indicatorActiveColor : _indicatorInactiveColor;
            }
            foreach (var indicator in _northSouthIndicators)
            {
                indicator.color = _crossRoads.CurrentLight == TrafficLightState.NorthSouth ? _indicatorActiveColor : _indicatorInactiveColor;
            }
        }
    }
}
