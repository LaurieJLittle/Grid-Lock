using System;
using System.Collections.Generic;
using UnityEngine;

public class RoadSegment
{
    private int _occupiedUnits;
    private int _reservedUnits;
    private readonly List<Vehicle> _vehicles = new List<Vehicle>();
    
    public int Id { get; }
    public int Capacity { get; }
    public Direction Direction { get; }
    public CrossRoads FromCrossRoads { get; set; }
    public CrossRoads ToCrossRoads { get; set; }
    private int FreeUnits => Capacity - _occupiedUnits - _reservedUnits;
    
    public event Action<float, VehicleConfig> OnSpawnPending;

    public RoadSegment(int id, int capacity, Direction direction)
    {
        Id = id;
        Capacity = capacity;
        Direction = direction;
    }

    public void MarkSpawnPending(float timeTillSpawn, VehicleConfig vehicleConfig)
    {
        OnSpawnPending?.Invoke(timeTillSpawn, vehicleConfig);
    }
    
    public void ReserveSpace(int units)
    {
        _reservedUnits += units;
    }
    
    public bool HasSpace(int units)
    {
        return FreeUnits >= units;
    }

    public float GetMaxVehicleProgress(Vehicle vehicle)
    {
        int distanceFromEnd = 0;
        for (int i = 0; i < _vehicles.Count; i++)
        {
            if (_vehicles[i].Id == vehicle.Id)
            {
                break;
            }

            distanceFromEnd += _vehicles[i].VehicleConfig.Size;
        }
        
        return  (float) (Capacity - distanceFromEnd) / Capacity; 
    }
    
    public void AddVehicle(Vehicle vehicle, bool freeReservedSpace = false)
    {
        if (freeReservedSpace)
        {
            _reservedUnits -= vehicle.VehicleConfig.Size;
        }
        
        if (!HasSpace(vehicle.VehicleConfig.Size))
        {
            Debug.LogError("Error: Adding vehicle to road Segment but no space available!");
        }

        _vehicles.Add(vehicle);
        _occupiedUnits += vehicle.VehicleConfig.Size;
    }

    public void RemoveVehicle(Vehicle vehicle)
    {
        if (_vehicles.Remove(vehicle))
        {
            _occupiedUnits -= vehicle.VehicleConfig.Size;
        }
    }
}
