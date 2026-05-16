using System.Collections.Generic;
using UnityEngine;

namespace GridLock.View
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float _padding = 0.5f;
        [SerializeField] private List<Transform> _environmentTransforms;
        
        private Vector2 _minXY = new Vector2(float.MaxValue, float.MaxValue);
        private Vector2 _maxXY = new Vector2(float.MinValue, float.MinValue);
        private Camera _cam;
        
        private void Awake()
        {
            _cam = GetComponent<Camera>();
        }

        private void Start()
        {
            if (_environmentTransforms.Count == 0)
            {
                return;
            }

            FitToTransforms(_environmentTransforms);
        }

        public void FitToTransforms(IReadOnlyList<Transform> transforms)
        {
            _minXY = new Vector2(float.MaxValue, float.MaxValue);
            _maxXY = new Vector2(float.MinValue, float.MinValue);

            foreach (var t in transforms)
            {
                _minXY.x = Mathf.Min(t.position.x, _minXY.x);
                _minXY.y = Mathf.Min(t.position.y, _minXY.y);

                _maxXY.x = Mathf.Max(t.position.x, _maxXY.x);
                _maxXY.y = Mathf.Max(t.position.y, _maxXY.y);
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
