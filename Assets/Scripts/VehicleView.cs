using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class VehicleView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _turnIndicator;
    [SerializeField] private Sprite _leftTurnSprite;
    [SerializeField] private Sprite _straightTurnSprite;
    [SerializeField] private Sprite _rightTurnSprite;
    
    private Vehicle _vehicle;
    private RoadNetworkView _roadNetworkView;

    private quaternion _targetRotation;
    private float _currentRotation;
    private readonly float _turnSpeed = 240f;
    
    public void SetData(Vehicle targetVehicle, RoadNetworkView roadNetworkView)
    {
        _vehicle = targetVehicle;
        _vehicle.TripComplete += OnTripComplete;
        _roadNetworkView = roadNetworkView;
        _spriteRenderer.sprite = _vehicle.VehicleConfig.Sprite;
        _vehicle.OnStateChanged += OnVehicleStateChanged;

        OnVehicleStateChanged(_vehicle, _vehicle.State);
        transform.rotation = _targetRotation;
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
            var dir = endPosition - startPosition;
            _targetRotation = quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) - (90 * Mathf.Deg2Rad));
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
                transform.position = Vector3.Lerp(startPosition, endPosition, progress);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, _turnSpeed * Time.deltaTime);
            }
        }
    }

    private bool TryGetCurrentPathData(out Vector3 startPosition, out Vector3 endPosition, out float progress)
    {
        if (_vehicle.CurrentSegment != null)
        {
            _roadNetworkView.GetSegmentStartEnd(_vehicle.CurrentSegment, out startPosition, out endPosition);
            progress = _vehicle.SegmentProgress;
            return true;
        }
        else if (_vehicle.TraversingCrossRoads != null)
        {
            _roadNetworkView.GetCrossRoadsStartEnd(_vehicle, _vehicle.TraversingCrossRoads, out startPosition, out endPosition);
            progress = (_vehicle.FullTraversalTime - _vehicle.CrossroadsTraversalTimeLeft) / _vehicle.FullTraversalTime;
            return true;
        }

        startPosition = new Vector3();
        endPosition = new Vector3();
        progress = 0;
        return false;
    }
    
    private void OnTripComplete()
    {
        Destroy(gameObject);
    }
}
