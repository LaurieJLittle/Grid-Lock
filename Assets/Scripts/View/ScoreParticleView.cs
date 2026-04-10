using System;
using System.Collections;
using GridLock.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GridLock.View
{
    public class ScoreParticleView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _lifetime = 1f;
        [SerializeField] private float _pathVariationMagnitude = 2f;

        public void MoveToScoreUI(Vector3 startWorldPosition, Vector3 endWorldPosition, Material material, Action onArrive)
        {
            transform.position = startWorldPosition;
            if (material != null)
            {
                _spriteRenderer.sharedMaterial = material;
            }
            StartCoroutine(Animate(startWorldPosition, endWorldPosition, onArrive));
        }

        private IEnumerator Animate(Vector3 start, Vector3 end, Action onArrive)
        {
            float elapsed = 0f;
            Vector3 mid = (start + end) * 0.5f;
            mid += Vector3.left * _pathVariationMagnitude * Random.Range(-1f, 1f);
            mid += Vector3.up * _pathVariationMagnitude * Random.Range(-1f, 1f);

            while (elapsed < _lifetime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _lifetime);
                transform.position = MathUtility.QuadraticBezier(start, mid, end, t);

                yield return null;
            }

            onArrive?.Invoke();
            Destroy(gameObject);
        }
    }
}
