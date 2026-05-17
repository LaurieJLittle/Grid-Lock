using System.Collections.Generic;
using GridLock.Core;
using GridLock.Simulation;
using GridLock.Utility;
using UnityEngine;

namespace GridLock.View
{
    public class RoadNetworkView : MonoBehaviour
    {
        [SerializeField] float _turnPathSmoothingFactor = 0.3f;
        [SerializeField] private CrossRoadView[] _crossRoadsViews;
        [SerializeField] private RoadSegmentView[] _roadSegmentViews;
        
        private readonly Dictionary<RoadSegment, RoadSegmentView> _roadViewLookUp = new Dictionary<RoadSegment, RoadSegmentView>();
        private readonly Dictionary<CrossRoads, CrossRoadView> _crossRoadsViewLookUp = new Dictionary<CrossRoads, CrossRoadView>();
        
        public void SetViews(CrossRoadView[] crossRoads, RoadSegmentView[] segments)
        {
            _crossRoadsViews = crossRoads;
            _roadSegmentViews = segments;
        }

        public void Init(IReadOnlyRoadNetwork network)
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

        public float GetSegmentWorldLength(RoadSegment roadSegment)
        {
            return _roadViewLookUp[roadSegment].WorldLength;
        }

        public Vector3 GetSegmentForwardDirection(RoadSegment roadSegment)
        {
            return _roadViewLookUp[roadSegment].ForwardDirection;
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
        public void GetCrossRoadsPathData(Vehicle vehicle, CrossRoads crossRoads, out Vector3 startPosition, out Vector3 bezierMidPoint, out Vector3 endPosition, out Vector3 inboundDir, out Vector3 outboundDir)
        {
            var inboundRoadDirEnum = NavigationUtility.GetOpposite(vehicle.GetNextStep().ApproachDirection);
            var outboundRoadDirEnum = vehicle.GetNextStep().ExitDirection;

            RoadSegment inboundRoadSegment = crossRoads.InboundRoads[inboundRoadDirEnum];
            RoadSegment outboundRoadSegment = crossRoads.OutboundRoads[outboundRoadDirEnum];

            var inboundRoadView = _roadViewLookUp[inboundRoadSegment];
            var outboundRoadView = _roadViewLookUp[outboundRoadSegment];

            startPosition = inboundRoadView.RoadEndPosition;
            endPosition = outboundRoadView.RoadStartPosition;

            inboundDir = inboundRoadView.ForwardDirection;
            outboundDir = outboundRoadView.ForwardDirection;

            if (inboundRoadDirEnum == outboundRoadDirEnum)
            {
                bezierMidPoint = Vector3.zero;
                return;
            }

            if (!MathUtility.TryFindLinesIntersection(startPosition, inboundDir, endPosition, outboundDir, out bezierMidPoint))
            {
                Debug.LogError("trying to find intersection point of parallel lines");
            }
            
            Vector3 linearMid = (startPosition + endPosition) * 0.5f;
            bezierMidPoint = Vector3.Lerp(bezierMidPoint, linearMid, _turnPathSmoothingFactor);
        }

    }
}
