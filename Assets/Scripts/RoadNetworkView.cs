using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadNetworkView : MonoBehaviour
{
    [SerializeField] private CrossRoadView[] _crossRoadsViews;
    [SerializeField] private RoadSegmentView[] _roadSegmentViews;
    
    private readonly Dictionary<RoadSegment, RoadSegmentView> _roadViewLookUp = new Dictionary<RoadSegment, RoadSegmentView>();
    private readonly Dictionary<CrossRoads, CrossRoadView> _crossRoadsViewLookUp = new Dictionary<CrossRoads, CrossRoadView>();
    
    public void Init(RoadNetwork network)
    {
        foreach (var crossRoadView in _crossRoadsViews)
        {
            crossRoadView.BindToNetwork(network.CrossRoads[crossRoadView.Id]);
            _crossRoadsViewLookUp.Add(network.CrossRoads[crossRoadView.Id], crossRoadView);
        }  
        
        foreach (var roadSegmentView in _roadSegmentViews)
        {
            roadSegmentView.BindToNetwork(network.Segments[roadSegmentView.Id]);
            _roadViewLookUp.Add(network.Segments[roadSegmentView.Id], roadSegmentView);
        }
    }

    public void GetSegmentStartEnd(RoadSegment roadSegment, out Vector3 startPosition, out Vector3 endPosition)
    {
        var roadView = _roadViewLookUp[roadSegment];
        startPosition = roadView.RoadStartPosition;
        endPosition = roadView.RoadEndPosition;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vehicle"></param>
    /// <param name="crossRoads"></param>
    /// <param name="startPosition"></param>
    /// <param name="endPosition"></param>
    /// <param name="bezierMidPoint"> can be used in a quadratic bezier function to produce a smooth turn </param> 
    /// <param name="isStraight"></param>
    public void GetCrossRoadsPathData(Vehicle vehicle, CrossRoads crossRoads, out Vector3 startPosition, out Vector3 bezierMidPoint, out Vector3 endPosition)
    {
        var inboundRoadDir = NavigationUtility.GetOpposite(vehicle.GetNextStep().ApproachDirection);
        var outboundRoadDir = vehicle.GetNextStep().ExitDirection;

        RoadSegment inboundRoadSegment = crossRoads.InboundRoads[inboundRoadDir];
        RoadSegment outboundRoadSegment = crossRoads.OutboundRoads[outboundRoadDir];

        var inboundRoadView = _roadViewLookUp[inboundRoadSegment];
        var outboundRoadView = _roadViewLookUp[outboundRoadSegment];

        startPosition = inboundRoadView.RoadEndPosition;
        endPosition = outboundRoadView.RoadStartPosition;

        if (inboundRoadDir == outboundRoadDir)
        {
            bezierMidPoint = Vector3.zero;
            return;
        }

        Vector3 inboundDir = (inboundRoadView.RoadEndPosition - inboundRoadView.RoadStartPosition).normalized;
        Vector3 outboundDir = (outboundRoadView.RoadEndPosition - outboundRoadView.RoadStartPosition).normalized;

        if (!MathUtility.TryFindLinesIntersection(startPosition, inboundDir, endPosition, outboundDir, out bezierMidPoint))
        {
            Debug.LogError("trying to find intersection point of parallel lines");
        }
    }

}
