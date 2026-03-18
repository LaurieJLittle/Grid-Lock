using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreText;
    
    private int _score;
    private SimulationManager _simulationManager;
    
    public void SetData(SimulationManager simulationManager)
    {
        _simulationManager = simulationManager;
        simulationManager.OnVehicleDestroyed += AddPointsForCompletedTrip;
        _scoreText.text = $"Score: {_score}";
    }

    private void AddPointsForCompletedTrip(Vehicle vehicle)
    {
        _score += 1;
        _scoreText.text = $"Score: {_score}";
    }

    private void OnDestroy()
    {
        if (_simulationManager != null)
        {
            _simulationManager.OnVehicleDestroyed -= AddPointsForCompletedTrip;
        }
    }
}
