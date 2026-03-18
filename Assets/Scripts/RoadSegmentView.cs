using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RoadSegmentView : MonoBehaviour
{
    [SerializeField] private RoadSegmentConfig _roadSegmentConfig;
    [SerializeField] private Transform _roadStart;
    [SerializeField] private Transform _roadEnd;
    [SerializeField] private SpriteRenderer _spawnIndicator;
    
    private RoadSegment _roadSegment;
    private Coroutine _highlightCoroutine;
    
    public int Id => _roadSegmentConfig.Id.GetHashCode();
    public Vector3 RoadStartPosition => _roadStart.position;
    public Vector3 RoadEndPosition => _roadEnd.position;

    public void BindToNetwork(RoadSegment roadSegment)
    {
        _roadSegment = roadSegment;
        _roadSegment.OnSpawnPending += HighlightSpawnIndicator;
    }

    private void HighlightSpawnIndicator(float timeTillSpawn)
    {
        if (_spawnIndicator != null)
        {
            if (_highlightCoroutine != null)
            {
                StopCoroutine(_highlightCoroutine);
            }
            
            _highlightCoroutine = StartCoroutine(IndicateIncoming(timeTillSpawn));
        }
    }

    IEnumerator IndicateIncoming(float timeTillSpawn)
    {
        _spawnIndicator.color = Color.green;
        yield return new WaitForSeconds(timeTillSpawn);
        _spawnIndicator.color = Color.black;
    }

    private void OnDestroy()
    {
        if (_roadSegment != null)
        {
            _roadSegment.OnSpawnPending -= HighlightSpawnIndicator;
        }
    }
}
