using UnityEngine;

namespace GridLock.Config
{
    [CreateAssetMenu(fileName = "VehicleConfig", menuName = "Scriptable Objects/Vehicle Config")]
    public class VehicleConfig : ScriptableObject, IVehicleConfig
    {
        [Header("Simulation Fields")] // needed for simulation (may also be used in visual layer as well)
        [SerializeField] private VehicleId _id;
        [SerializeField] private int _size = 1;

        [Header("Visual Fields")] // not needed for simulation
        [SerializeField] private float _centreToFrontDistance = 0.1f;
        [SerializeField] private Sprite[] _rotationSprites; // LL TODO - expectation is 37 sprites at 5 degree intervals, making a 180 turn - should probably add unit testing for this

        public VehicleId Id => _id;
        public int Size => _size;
        public float CentreToFrontDistance => _centreToFrontDistance; // used in vehicle view to calculate offset needed so car stops infront of line at crossroads
        public Sprite[] RotationSprites => _rotationSprites;
    }
    
    public enum VehicleId
    {
        BlueCar,
        RedCar,
        GreenCar,
        OrangeCar,
        PurpleCar,
        TealCar,
        WhiteCar,
        YellowCar,
        Bus,
    }
}
