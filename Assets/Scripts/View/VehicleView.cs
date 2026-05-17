using System;
using System.Collections;
using GridLock.Config;
using GridLock.Core;
using GridLock.Simulation;
using GridLock.Utility;
using UnityEngine;

namespace GridLock.View
{
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
        [SerializeField] private float _spawnFailedDuration = 0.5f;
        [SerializeField] private float _spawnFailedShakeIntensity = 0.05f;
        [SerializeField] private float _spawnFailedShakeFrequency = 30f;
        [SerializeField] private SpriteRenderer _spawnFailedIcon;
        [SerializeField] private float _exitFadeDuration = 0.5f;
        [SerializeField] private float _compressedIndicatorAxisScale = 0.5f;
        [SerializeField] private float _regularIndicatorAxisScale = 1f;
        [SerializeField] private float _indicatorForwardOffset = 0.3f;

        private Vehicle _vehicle;
        private VehicleConfig _vehicleConfig;
        private RoadNetworkView _roadNetworkView;
        private RoundViewManager _roundViewManager;
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
        private bool _isFadingOut;

        private void Start()
        {
            _sinCameraElevation = Mathf.Sin(kCameraElevationDeg * Mathf.Deg2Rad);
        }

        public void SetData(Vehicle targetVehicle, VehicleConfig vehicleConfig, RoadNetworkView roadNetworkView, RoundViewManager roundViewManager)
        {
            _vehicle = targetVehicle;
            _vehicleConfig = vehicleConfig;
            _vehicle.TripComplete += OnTripComplete;
            _roadNetworkView = roadNetworkView;
            _roundViewManager = roundViewManager;
            _rotationSprites = _vehicleConfig.RotationSprites;
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

        public void ActivateFromPreview(Vehicle vehicle, VehicleConfig vehicleConfig, RoadNetworkView roadNetworkView, RoundViewManager roundViewManager)
        {
            if (_previewCoroutine != null)
            {
                StopCoroutine(_previewCoroutine);
                _previewCoroutine = null;
            }

            _spriteRenderer.color = Color.white;
            SetData(vehicle, vehicleConfig, roadNetworkView, roundViewManager);
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
            _spawnFailedIcon.gameObject.SetActive(true);
            Vector3 originalPosition = transform.position;
            float elapsed = 0f;

            while (elapsed < _spawnFailedDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _spawnFailedDuration;
                float shake = Mathf.Sin(elapsed * _spawnFailedShakeFrequency * Mathf.PI * 2f) * _spawnFailedShakeIntensity * (1f - t);
                transform.position = originalPosition + new Vector3(shake, 0f, 0f);

                float alpha = 1f - t;
                Color c = _spriteRenderer.color;
                c.a = alpha;
                _spriteRenderer.color = c;

                Color ic = _spawnFailedIcon.color;
                ic.a = alpha;
                _spawnFailedIcon.color = ic;

                yield return null;
            }

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

                if (_vehicle.CurrentSegment != null)
                {
                    Direction travelDirection = _vehicle.CurrentSegment.Direction;
                    Vector3 forwardDir = _roadNetworkView.GetSegmentForwardDirection(_vehicle.CurrentSegment);
                    ApplyTurnIndicatorTransform(travelDirection, forwardDir);
                }
            }
            else
            {
                _turnIndicator.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (_vehicle != null && !_isFadingOut)
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
        }

        private void ApplyTurnIndicatorTransform(Direction travelDirection, Vector3 forwardDir)
        {
            float rotation;
            float scaleX;
            float scaleY;

            switch (travelDirection)
            {
                case Direction.North:
                    rotation = 0f;
                    scaleX = _regularIndicatorAxisScale;
                    scaleY = _compressedIndicatorAxisScale;
                    break;
                case Direction.East:
                    rotation = -90f;
                    scaleX = _compressedIndicatorAxisScale;
                    scaleY = _regularIndicatorAxisScale;
                    break;
                case Direction.South:
                    rotation = 180f;
                    scaleX = _regularIndicatorAxisScale;
                    scaleY = _compressedIndicatorAxisScale;
                    break;
                case Direction.West:
                    rotation = 90f;
                    scaleX = _compressedIndicatorAxisScale;
                    scaleY = _regularIndicatorAxisScale;
                    break;
                default:
                    Debug.LogError("invalid travel direction detected, turn indicator not configured");
                    rotation = 0f;
                    scaleX = _regularIndicatorAxisScale;
                    scaleY = _regularIndicatorAxisScale;
                    break;
            }

            _turnIndicator.transform.localPosition = forwardDir * _indicatorForwardOffset * (1 + _vehicle.VehicleConfig.Size);
            _turnIndicator.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            _turnIndicator.transform.localScale = new Vector3(scaleX, scaleY, 1f);
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
                Vector3 dir = _roadNetworkView.GetSegmentForwardDirection(_vehicle.CurrentSegment);
                float offsetMagnitude = _vehicleConfig.CentreToFrontDistance;
                startPosition -= dir * offsetMagnitude;
                endPosition -= dir * offsetMagnitude;
                
                progress = _vehicle.Progress;
                _hasBezierPath = false;
                return true;
            }
            else if (_vehicle.TraversingCrossRoads != null)
            {
                _roadNetworkView.GetCrossRoadsPathData(_vehicle, _vehicle.TraversingCrossRoads, out startPosition, out _bezierMidPoint, out endPosition, out Vector3 inboundDir, out Vector3 outboundDir);
                
                // offset vehicle by distance from front to centre of vehicle so front of vehicle reflects progress value not centre
                float offsetMagnitude = _vehicleConfig.CentreToFrontDistance;
                _crossRoadsStartOffset = -inboundDir * offsetMagnitude;
                _crossRoadsEndOffset = -outboundDir * offsetMagnitude;
                startPosition += _crossRoadsStartOffset;
                endPosition += _crossRoadsEndOffset;
                
                progress = _vehicle.Progress;
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
            _vehicle.TripComplete -= OnTripComplete;
            _vehicle.OnStateChanged -= OnVehicleStateChanged;

            if (_roundViewManager != null)
            {
                _roundViewManager.SpawnScoreParticle(transform.position, _vehicle.VehicleConfig.Id, 1);
            }

            _isFadingOut = true;

            float worldSpeed = 0f;
            if (_vehicle.CurrentSegment != null)
            {
                float worldLength = _roadNetworkView.GetSegmentWorldLength(_vehicle.CurrentSegment);
                worldSpeed = _vehicle.MovementConfig.Speed * worldLength / _vehicle.CurrentSegment.Capacity;
            }

            float angle = _currentAngle * Mathf.Deg2Rad;
            Vector3 exitDirection = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0f);
            StartCoroutine(FadeOutAndDestroy(exitDirection, worldSpeed));
        }

        private IEnumerator FadeOutAndDestroy(Vector3 direction, float speed)
        {
            float elapsed = 0f;

            while (elapsed < _exitFadeDuration)
            {
                elapsed += Time.deltaTime;
                transform.position += direction * speed * Time.deltaTime;

                float alpha = 1f - (elapsed / _exitFadeDuration);
                Color c = _spriteRenderer.color;
                c.a = alpha;
                _spriteRenderer.color = c;

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
