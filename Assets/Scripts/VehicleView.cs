using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class VehicleView : MonoBehaviour
{
    private const float kCameraElevationDeg = 45f;
    
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _turnIndicator;
    [SerializeField] private Sprite _leftTurnSprite;
    [SerializeField] private Sprite _straightTurnSprite;
    [SerializeField] private Sprite _rightTurnSprite; 
    [SerializeField] private float _previewPulseFrequency = 4f;
    [SerializeField] private float _previewPulseAlphaMin = 0.3f;
    [SerializeField] private float _previewPulseAlphaMax = 0.7f;
    [SerializeField] private float _spawnFailedDisplayDuration = 0.5f;

    private Vehicle _vehicle;
    private RoadNetworkView _roadNetworkView;
    private Sprite[] _rotationSprites;

    private float _targetAngle;
    private float _currentAngle;
    private readonly float _turnSpeed = 100; // LL - TODO consider whether this is still nessecary now we have Bezier curves

    private float _sinCameraElevation;
    private Vector3 _bezierMidPoint;
    private bool _hasBezierPath;
    private Vector3 _crossRoadsStartOffset;
    private Vector3 _crossRoadsEndOffset;
    private Coroutine _previewCoroutine;

    private void Start()
    {
        _sinCameraElevation = Mathf.Sin(kCameraElevationDeg * Mathf.Deg2Rad);
    }

    public void SetData(Vehicle targetVehicle, RoadNetworkView roadNetworkView)
    {
        _vehicle = targetVehicle;
        _vehicle.TripComplete += OnTripComplete;
        _roadNetworkView = roadNetworkView;
        _rotationSprites = _vehicle.VehicleConfig.RotationSprites;
        _vehicle.OnStateChanged += OnVehicleStateChanged;

        OnVehicleStateChanged(_vehicle, _vehicle.State);
        _currentAngle = _targetAngle;
        UpdateSpriteFromAngle();
    }

    public void SetPreviewData(VehicleConfig vehicleConfig, Vector3 position, Vector3 direction)
    {
        _sinCameraElevation = Mathf.Sin(kCameraElevationDeg * Mathf.Deg2Rad);
        _rotationSprites = vehicleConfig.RotationSprites;

        transform.position = position;
        _targetAngle = DirectionToAngle(direction);
        _currentAngle = _targetAngle;
        UpdateSpriteFromAngle();

        _turnIndicator.gameObject.SetActive(false);
        _previewCoroutine = StartCoroutine(PulsePreview());
    }

    public void ActivateFromPreview(Vehicle vehicle, RoadNetworkView roadNetworkView)
    {
        if (_previewCoroutine != null)
        {
            StopCoroutine(_previewCoroutine);
            _previewCoroutine = null;
        }

        _spriteRenderer.color = Color.white;
        SetData(vehicle, roadNetworkView);
    }

    public void ShowSpawnFailed()
    {
        if (_previewCoroutine != null)
        {
            StopCoroutine(_previewCoroutine);
            _previewCoroutine = null;
        }

        StartCoroutine(SpawnFailedAnimation());
    }

    private IEnumerator PulsePreview()
    {
        float t = 0f;
        while (true)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(_previewPulseAlphaMin, _previewPulseAlphaMax, (Mathf.Sin(t * Mathf.PI * _previewPulseFrequency) + 1f) * 0.5f);
            Color c = _spriteRenderer.color;
            c.a = alpha;
            _spriteRenderer.color = c;
            yield return null;
        }
    }

    private IEnumerator SpawnFailedAnimation()
    {
        _spriteRenderer.color = new Color(1f, 0f, 0f, 0.5f);
        yield return new WaitForSeconds(_spawnFailedDisplayDuration);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_vehicle == null)
        {
            return;
        }

        _vehicle.TripComplete -= OnTripComplete;
        _vehicle.OnStateChanged -= OnVehicleStateChanged;
    }

    void OnVehicleStateChanged(Vehicle vehicle, VehicleState vehicleState)
    {
        if (TryGetCurrentPathData(out Vector3 startPosition, out Vector3 endPosition, out float progress))
        {
            if (!_hasBezierPath) // the case where we do have a bezier is handled in update because we need to update every frame
            {
                var dir = endPosition - startPosition;
                _targetAngle = DirectionToAngle(dir);
            }
        }

        if (vehicleState == VehicleState.Queued)
        {
            var turn = _vehicle.GetNextStep().Turn;
            _turnIndicator.gameObject.SetActive(true);
            switch (turn)
            {
                case TurnDirection.Left:
                    _turnIndicator.sprite = _leftTurnSprite;
                    break;
                case TurnDirection.Right:
                    _turnIndicator.sprite = _rightTurnSprite;
                    break;
                case TurnDirection.Straight:
                    _turnIndicator.sprite = _straightTurnSprite;
                    break;
            }
        }
        else
        {
            _turnIndicator.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (_vehicle != null)
        {
            if (TryGetCurrentPathData(out Vector3 startPosition, out Vector3 endPosition, out float progress))
            {
                if (_hasBezierPath)
                {
                    // need to lerp between vehicle offset on previous road to vehicle offset on new road as well for smooth turn.
                    Vector3 unshiftedStart = startPosition - _crossRoadsStartOffset;
                    Vector3 unshiftedEnd = endPosition - _crossRoadsEndOffset;
                    
                    transform.position = MathUtility.QuadraticBezier(unshiftedStart, _bezierMidPoint, unshiftedEnd, progress)
                        + Vector3.Lerp(_crossRoadsStartOffset, _crossRoadsEndOffset, progress);
                    Vector3 tangent = MathUtility.QuadraticBezierTangent(unshiftedStart, _bezierMidPoint, unshiftedEnd, progress);
                    _targetAngle = DirectionToAngle(tangent);
                }
                else
                {
                    transform.position = Vector3.Lerp(startPosition, endPosition, progress);
                }
                _currentAngle = Mathf.MoveTowardsAngle(_currentAngle, _targetAngle, _turnSpeed * Time.deltaTime);
                UpdateSpriteFromAngle();
            }
        }
    }

    private void UpdateSpriteFromAngle()
    {
        float angle = _currentAngle % 360f;
        if (angle < 0f)
        {
            angle += 360f;
        }

        bool flip;
        float spriteAngle;

        if (angle <= 180f)
        {
            spriteAngle = angle;
            flip = true;
        }
        else
        {
            spriteAngle = 360f - angle;
            flip = false;
        }

        float spriteAngleRad = spriteAngle * Mathf.Deg2Rad;
        float worldAngle = Mathf.Atan2(Mathf.Sin(spriteAngleRad) * _sinCameraElevation, Mathf.Cos(spriteAngleRad)) * Mathf.Rad2Deg;
        if (worldAngle < 0f)
        {
            worldAngle += 360f;
        }

        float degreesPerSprite = 180f / (_rotationSprites.Length - 1);
        int index = Mathf.Clamp(Mathf.RoundToInt((180f - worldAngle) / degreesPerSprite), 0, _rotationSprites.Length - 1);
        _spriteRenderer.sprite = _rotationSprites[index];
        _spriteRenderer.flipX = flip;

        _turnIndicator.transform.rotation = Quaternion.Euler(0f, 0f, -_currentAngle);
    }

    private float DirectionToAngle(Vector3 dir)
    {
        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        if (angle < 0f)
        {
            angle += 360f;
        }
        return angle;
    }

    private bool TryGetCurrentPathData(out Vector3 startPosition, out Vector3 endPosition, out float progress)
    {
        if (_vehicle.CurrentSegment != null)
        {
            _roadNetworkView.GetSegmentStartEnd(_vehicle.CurrentSegment, out startPosition, out endPosition);
            
            // offset vehicle by distance from front to centre of vehicle so front of vehicle reflects progress value not centre
            // this means e.g, when stopping at a cross roads the front of the vehicle will be on the give way line, not the middle
            Vector3 dir = (endPosition - startPosition).normalized;
            float offsetMagnitude = _vehicle.VehicleConfig.CentreToFrontDistance;
            startPosition -= dir * offsetMagnitude;
            endPosition -= dir * offsetMagnitude;
            
            progress = _vehicle.SegmentProgress;
            _hasBezierPath = false;
            return true;
        }
        else if (_vehicle.TraversingCrossRoads != null)
        {
            _roadNetworkView.GetCrossRoadsPathData(_vehicle, _vehicle.TraversingCrossRoads, out startPosition, out _bezierMidPoint, out endPosition, out Vector3 inboundDir, out Vector3 outboundDir);
            
            // offset vehicle by distance from front to centre of vehicle so front of vehicle reflects progress value not centre
            float offsetMagnitude = _vehicle.VehicleConfig.CentreToFrontDistance;
            _crossRoadsStartOffset = -inboundDir * offsetMagnitude;
            _crossRoadsEndOffset = -outboundDir * offsetMagnitude;
            startPosition += _crossRoadsStartOffset;
            endPosition += _crossRoadsEndOffset;
            
            progress = (_vehicle.FullTraversalTime - _vehicle.CrossroadsTraversalTimeLeft) / _vehicle.FullTraversalTime;
            _hasBezierPath = _vehicle.GetNextStep().Turn != TurnDirection.Straight;
            return true;
        }

        startPosition = new Vector3();
        endPosition = new Vector3();
        progress = 0;
        _hasBezierPath = false;
        return false;
    }


    private void OnTripComplete()
    {
        Destroy(gameObject);
    }
}
