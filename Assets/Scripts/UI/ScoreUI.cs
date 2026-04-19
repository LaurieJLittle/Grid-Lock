using System.Collections;
using GridLock.Simulation;
using TMPro;
using UnityEngine;

namespace GridLock.UI
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private float _punchScaleDuration = 0.2f;
        [SerializeField] private float _punchScaleAmount = 1.3f;

        private int _score; // true score value, incremented immediately when a vehicle exits the network.
        private int _displayScore; // incremented on particles hitting the ScoreUI, to give nice visual feedback.
        private SimulationManager _simulationManager;
        private Coroutine _punchCoroutine;
        private RectTransform _rectTransform;
        private Vector2 _originalAnchoredPosition;
        private Vector2 _originalAnchorMin;
        private Vector2 _originalAnchorMax;
        private Vector2 _originalPivot;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalAnchoredPosition = _rectTransform.anchoredPosition;
            _originalAnchorMin = _rectTransform.anchorMin;
            _originalAnchorMax = _rectTransform.anchorMax;
            _originalPivot = _rectTransform.pivot;
        }

        public void SetData(SimulationManager simulationManager)
        {
            _simulationManager = simulationManager;
            _simulationManager.OnVehicleDestroyed += AddPointsForCompletedTrip;
            _scoreText.text = $"Score: {_displayScore}";
        }

        private void AddPointsForCompletedTrip(Vehicle vehicle)
        {
            _score += 1;
        }

        private void OnDestroy()
        {
            if (_simulationManager != null)
            {
                _simulationManager.OnVehicleDestroyed -= AddPointsForCompletedTrip;
            }
        }

        public void AddToDisplayScore(int points)
        {
            _displayScore += points;
            _scoreText.text = $"Score: {_displayScore}";

            if (_punchCoroutine != null)
            {
                StopCoroutine(_punchCoroutine);
            }
            _punchCoroutine = StartCoroutine(PunchScale());
        }

        public void ShiftToCentre()
        {
            _rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _rectTransform.anchoredPosition = Vector2.zero;
            _scoreText.alignment = TextAlignmentOptions.Center;
        }

        public void ResetPosition()
        {
            _rectTransform.anchorMin = _originalAnchorMin;
            _rectTransform.anchorMax = _originalAnchorMax;
            _rectTransform.pivot = _originalPivot;
            _rectTransform.anchoredPosition = _originalAnchoredPosition;
            _scoreText.alignment = TextAlignmentOptions.Right;
        }

        private IEnumerator PunchScale()
        {
            float elapsed = 0f;
            Vector3 baseScale = Vector3.one;
            Vector3 peakScale = Vector3.one * _punchScaleAmount;

            while (elapsed < _punchScaleDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _punchScaleDuration);
                // Sin arc: 0 -> 1 -> 0, so scale pings up to peak and back.
                float curve = Mathf.Sin(t * Mathf.PI);
                _scoreText.transform.localScale = Vector3.LerpUnclamped(baseScale, peakScale, curve);
                yield return null;
            }

            _scoreText.transform.localScale = baseScale;
            _punchCoroutine = null;
        }
    }
}
