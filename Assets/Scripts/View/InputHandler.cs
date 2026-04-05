using UnityEngine;

namespace GridLock.View
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryClickIntersection(Input.mousePosition);
            }
        }

        private void TryClickIntersection(Vector2 screenPosition)
        {
            Vector3 worldPoint = _camera.ScreenToWorldPoint(screenPosition);
            Collider2D hit = Physics2D.OverlapPoint(worldPoint);
            
            if (hit != null && hit.TryGetComponent(out CrossRoadView view))
            {
                view.ToggleLight();
            }
        }
    }
}
