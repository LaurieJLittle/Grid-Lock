using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _turnIndicator;
    [SerializeField] private Sprite _leftTurnSprite;
    [SerializeField] private Sprite _straightTurnSprite;
    [SerializeField] private Sprite _rightTurnSprite;

    private Vehicle _vehicle;
    private RoadNetworkView _roadNetworkView;
    private Sprite[] _rotationSprites;

    private float _targetAngle;
    private float _currentAngle;
    private readonly float _turnSpeed = 240f; // LL - TODO consider whether this is still nessecary now we have Bezier curves

    private Vector3 _bezierMidPoint;
    private bool _hasBezierPath;

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

    private void OnDestroy()
    {
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
                    transform.position = MathUtility.QuadraticBezier(startPosition, _bezierMidPoint, endPosition, progress);
                    Vector3 tangent = MathUtility.QuadraticBezierTangent(startPosition, _bezierMidPoint, endPosition, progress);
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

        int index = Mathf.Clamp(Mathf.CeilToInt( (180 - spriteAngle) / 5), 0, _rotationSprites.Length - 1);
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
            progress = _vehicle.SegmentProgress;
            _hasBezierPath = false;
            return true;
        }
        else if (_vehicle.TraversingCrossRoads != null)
        {
            _roadNetworkView.GetCrossRoadsPathData(_vehicle, _vehicle.TraversingCrossRoads, out startPosition, out _bezierMidPoint, out endPosition);
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
