using UnityEngine;

namespace GridLock.Config
{
    [CreateAssetMenu(fileName = "VehicleMovementConfig", menuName = "Scriptable Objects/Vehicle Movement Config")]
    public class VehicleMovementConfig : ScriptableObject
    {
        [SerializeField] private float _leftTurnTraversalTime = 0.75f;
        [SerializeField] private float _rightTurnTraversalTime = 2f;
        [SerializeField] private float _straightTraversalTime = 1.5f;
        [SerializeField] private float _speed = 2f;

        public float LeftTurnTraversalTime => _leftTurnTraversalTime;
        public float RightTurnTraversalTime => _rightTurnTraversalTime;
        public float StraightTraversalTime => _straightTraversalTime;
        public float Speed => _speed;
    }
}
