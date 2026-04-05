using UnityEngine;

namespace GridLock.Config
{
    [CreateAssetMenu(fileName = "VehicleConfig", menuName = "Scriptable Objects/Vehicle Config")]
    public class VehicleConfig : ScriptableObject
    {
        [SerializeField] private Type _vehicleType;
        [SerializeField] private int _size = 1;
        [SerializeField] private float _leftTurnTraversalTime = 0.75f;
        [SerializeField] private float _rightTurnTraversalTime = 2f;
        [SerializeField] private float _straightTraversalTime = 1.5f;
        [SerializeField] private float _speed = 2f; // Speed / Capacity = time to fully traverse road segment
        [SerializeField] private float _centreToFrontDistance = 0.1f;
        [SerializeField] private Sprite[] _rotationSprites; // LL TODO - expectation is 37 sprites at 5 degree intervals, making a 180 turn - should probably add unit testing for this

        public enum Type
        {
            Car,
            Lorry,
        }

        public Type VehicleType => _vehicleType;
        public int Size => _size;
        public float LeftTurnTraversalTime => _leftTurnTraversalTime;
        public float RightTurnTraversalTime => _rightTurnTraversalTime;
        public float StraightTraversalTime => _straightTraversalTime;
        public float Speed => _speed;
        public float CentreToFrontDistance => _centreToFrontDistance; // used in vehicle view to calculate offset needed so car stops infront of line at crossroads
        public Sprite[] RotationSprites => _rotationSprites;
    }
}
