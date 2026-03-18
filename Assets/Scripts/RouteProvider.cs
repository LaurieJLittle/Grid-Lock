using System.Collections.Generic;
using UnityEngine;

public class RouteProvider
{
    public List<RouteStep> FindRoute(RoadSegment startSegment, RoadSegment targetExit)
    {
        var visited = new HashSet<int>();
        var queue = new Queue<RouteNode>();
        queue.Enqueue(new RouteNode(startSegment, null, default, startSegment.Direction, default));
        visited.Add(startSegment.Id);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current.Segment.Id == targetExit.Id)
            {
                // Route Found! return route
                return ReconstructRoute(current);
            }

            CrossRoads crossRoads = current.Segment.ToCrossRoads;
            if (crossRoads == null)
            {
                // found an exit but not the one we are looking for
                continue;
            }

            Direction approachDir = NavigationUtility.GetOpposite(current.Segment.Direction);

            // Try all outbound roads from this CrossRoads
            foreach (var kvp in crossRoads.OutboundRoads)
            {
                RoadSegment nextSegment = kvp.Value;
                if (visited.Contains(nextSegment.Id))
                {
                    continue;
                }

                if (approachDir == kvp.Key) // no U turns
                {
                    continue;
                }

                TurnDirection turn = NavigationUtility.DeduceTurn(approachDir, kvp.Key);
                visited.Add(nextSegment.Id);

                var step = new RouteNode(nextSegment, current, approachDir, kvp.Key, turn)
                {
                    ViaCrossRoads = crossRoads
                };
                queue.Enqueue(step);
            }
        }

        return null;
    }

    private List<RouteStep> ReconstructRoute(RouteNode endNode)
    {
        var steps = new List<RouteStep>();
        var current = endNode;

        while (current.Parent != null)
        {
            var step = new RouteStep(
                current.Segment,
                current.ViaCrossRoads,
                current.ApproachDirection,
                current.ExitDirection,
                current.Turn);
            steps.Add(step);
            current = current.Parent;
        }
        
        // Add the starting segment (no CrossRoads to pass through)
        steps.Add(new RouteStep(current.Segment, null, default, current.ExitDirection, default));
        steps.Reverse();
        return steps;
    }

    private class RouteNode
    {
        public RoadSegment Segment;
        public RouteNode Parent;
        public CrossRoads ViaCrossRoads;
        public Direction ApproachDirection;
        public Direction ExitDirection;
        public TurnDirection Turn;

        public RouteNode(RoadSegment segment, RouteNode parent,
            Direction approachDir, Direction exitDirection, TurnDirection turn)
        {
            Segment = segment;
            Parent = parent;
            ApproachDirection = approachDir;
            ExitDirection = exitDirection;
            Turn = turn;
        }
    }
}
