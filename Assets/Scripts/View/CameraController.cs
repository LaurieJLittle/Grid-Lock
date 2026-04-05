using System.Collections.Generic;
using UnityEngine;

namespace GridLock.View
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float _padding = 0.5f;
        [SerializeField] private List<Transform> _environmentTransforms;
        
        private Vector2 _minXY;
        private Vector2 _maxXY;
        private Camera _cam;
        
        private void Start()
        {
            _cam = GetComponent<Camera>();
            foreach (var environmentTransform in _environmentTransforms)
            {
                _minXY.x = Mathf.Min(environmentTransform.position.x, _minXY.x);
                _minXY.y = Mathf.Min(environmentTransform.position.y, _minXY.y);
                
                _maxXY.x = Mathf.Max(environmentTransform.position.x, _maxXY.x);
                _maxXY.y = Mathf.Max(environmentTransform.position.y, _maxXY.y);
            }
            
            UpdateBounds();
        }

        private void UpdateBounds()
        {
            float vertExtent = (_maxXY.y - _minXY.y) / 2f + _padding;
            float horizExtent = ((_maxXY.x - _minXY.x) / 2f + _padding) / _cam.aspect;

            var environmentCentre = (_minXY + _maxXY) / 2f;
            transform.position = new Vector3(environmentCentre.x, environmentCentre.y, transform.position.z);
            _cam.orthographicSize = Mathf.Max(vertExtent, horizExtent);
        }
    }
}
