using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "VehicleConfig", menuName = "Scriptable Objects/Vehicle Config")]
public class VehicleConfig : ScriptableObject
{
    [SerializeField] private Type _vehicleType;
    [SerializeField] private int _size = 1;
    [SerializeField] private float _leftTurnTraversalTime = 0.75f;
    [SerializeField] private float _rightTurnTraversalTime = 2f;
    [SerializeField] private float _straightTraversalTime = 1.5f;
    [SerializeField] private float _speed = 2f; // Speed / Capacity = time to fully traverse road segment
    [SerializeField] private Sprite _sprite;
    
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
    public Sprite Sprite => _sprite;
}
