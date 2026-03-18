using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NavigationUtility
{
    public static Direction GetOpposite(Direction dir)
    {
        if (dir == Direction.North) return Direction.South;
        if (dir == Direction.South) return Direction.North;
        if (dir == Direction.East) return Direction.West;
        if (dir == Direction.West) return Direction.East;

        Debug.LogError("couldn't find opposite direction");
        return Direction.None;
    }

    public static TurnDirection DeduceTurn(Direction approachFrom, Direction exitToward)
    {
        Direction straight = ResolveExitDirection(approachFrom, TurnDirection.Straight);
        if (exitToward == straight)
        {
            return TurnDirection.Straight;
        }

        Direction left = ResolveExitDirection(approachFrom, TurnDirection.Left);
        if (exitToward == left)
        {
            return TurnDirection.Left;
        }
        
        Direction right =  ResolveExitDirection(approachFrom, TurnDirection.Right);
        if (exitToward == right)
        {
            return TurnDirection.Right;
        }
        
        Debug.LogError($"Error determining turn direction - approachFrom {approachFrom}  exitToward {exitToward}");
        return TurnDirection.Right;
    }
    
    public static Direction ResolveExitDirection(Direction approachFrom, TurnDirection turn)
    {
        switch (approachFrom, turn)
        {
            case (Direction.South, TurnDirection.Straight):
            case (Direction.East, TurnDirection.Right): 
            case (Direction.West, TurnDirection.Left):
                return Direction.North;
            case (Direction.South, TurnDirection.Right): 
            case (Direction.North, TurnDirection.Left):
            case (Direction.West, TurnDirection.Straight):
                return Direction.East;
            case (Direction.North, TurnDirection.Straight):
            case (Direction.West, TurnDirection.Right):
            case (Direction.East, TurnDirection.Left):
                return Direction.South;
            case (Direction.North, TurnDirection.Right):
            case (Direction.South, TurnDirection.Left):
            case (Direction.East, TurnDirection.Straight):
                return Direction.West;
        }

        Debug.LogError("Could not resolve exit direction");
        return Direction.None;
    }
    
    public static bool AreTurnsConflicting(Direction approachA, TurnDirection turnA, Direction approachB, TurnDirection turnB)
    {
        // Same approach direction — sequential, not conflicting at the crossroads level
        // (queuing handles this on the road segment)
        if (approachA == approachB)
        {
            return false;
        }

        if (AreOpposite(approachA, approachB))
        {
            return AreOpposingMovementsConflicting(turnA, turnB);
        }

        // Perpendicular — always conflicting (different light phases)
        return true;
    }

    private static bool AreOpposite(Direction a, Direction b)
    {
        return (a == Direction.North && b == Direction.South || a == Direction.South && b == Direction.North ||
                a == Direction.East && b == Direction.West || a == Direction.West && b == Direction.East);
    }

    private static bool AreOpposingMovementsConflicting(TurnDirection turnA, TurnDirection turnB)
    {
        // right turns always conflict / have to be sequential with oncoming traffic as it moves across the opposing traffic lane
        return (turnA == TurnDirection.Right || turnB == TurnDirection.Right);
    }
}
