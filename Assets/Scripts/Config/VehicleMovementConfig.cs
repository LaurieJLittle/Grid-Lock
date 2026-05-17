using UnityEngine;

namespace GridLock.Config
{
    [CreateAssetMenu(fileName = "VehicleMovementConfig", menuName = "Scriptable Objects/Vehicle Movement Config")]
    public class VehicleMovementConfig : ScriptableObject, IVehicleMovementConfig
    {
        [SerializeField] private float _leftTurnDistance = 3f;
        [SerializeField] private float _rightTurnDistance = 4.5f;
        [SerializeField] private float _straightDistance = 3.75f;
        [SerializeField] private float _speed = 2f;

        public float LeftTurnDistance => _leftTurnDistance;
        public float RightTurnDistance => _rightTurnDistance;
        public float StraightDistance => _straightDistance;
        public float Speed => _speed;
    }
}
