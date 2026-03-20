using UnityEngine;

public static class MathUtility
{
    public static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * p0 + 2f * oneMinusT * t * p1 + t * t * p2;
    }

    public static Vector3 QuadraticBezierTangent(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        return 2f * (1f - t) * (p1 - p0) + 2f * t * (p2 - p1);
    }

    public static bool TryFindLinesIntersection(Vector3 p1, Vector3 d1, Vector3 p2, Vector3 d2, out Vector3 intersection)
    {
        float cross = d1.x * d2.y - d1.y * d2.x;
        if (Mathf.Abs(cross) < 0.0001f)
        {
            intersection = Vector3.zero;
            return false;
        }

        Vector3 diff = p2 - p1;
        float t = (diff.x * d2.y - diff.y * d2.x) / cross;
        intersection = p1 + d1 * t;
        return true;
    }
}
