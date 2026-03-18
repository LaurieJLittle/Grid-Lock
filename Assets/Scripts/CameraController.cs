using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private float _padding = 0.5f;
    [SerializeField] private Vector2 _minXY;
    [SerializeField] private Vector2 _maxXY;
    
    private Camera _cam;

    private void Start()
    {
        _cam = GetComponent<Camera>();
        UpdateBounds();
    }

    private void UpdateBounds()
    {
        float vertExtent = (_maxXY.y - _minXY.y) / 2f + _padding;
        float horizExtent = ((_maxXY.x - _minXY.x) / 2f + _padding) / _cam.aspect;
        
        _cam.orthographicSize = Mathf.Max(vertExtent, horizExtent);
    }
}
